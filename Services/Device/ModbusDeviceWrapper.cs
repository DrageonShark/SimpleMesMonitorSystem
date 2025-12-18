using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NModbus;
using NModbus.IO;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;
using WPF9SimpleMesMonitorSystem.Services.Device.States;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    /// <summary>
    /// 单个设备的通信包装器，负责 Modbus 连接与读取，并生成统一的快照。
    /// </summary>
    public class ModbusDeviceWrapper:IDisposable
    {
        public Models.Device DeviceInfo { get; private set; }

        private IModbusMaster? _master;
        private TcpClient? _tcpClient;
        private SerialPort? _serialPort;
        private bool _isConnected = false;
        private bool _useSimulation = false;
        private readonly DeviceStateContext _stateContext;
        private readonly Random _random = new();
        private double _simTemperature = 32d;
        private double _simPressure = 3.2d;
        private int _simSpeed = 500;
        private string _simStatus = "Stopped";

        public ModbusDeviceWrapper(Models.Device device, Action<string>? logAction = null, Action<string>? alarmAction = null)
        {
            DeviceInfo = device ?? throw new ArgumentNullException(nameof(device));
            _stateContext = new DeviceStateContext(DeviceInfo, logAction, alarmAction);
        }

        /// <summary>
        /// 连接设备 (自动识别 TCP 或 RTU)
        /// </summary>
        /// <returns>返回连接是否成功</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                DisposeConnections();

                var factory = new ModbusFactory();
                if (!string.IsNullOrWhiteSpace(DeviceInfo.IpAddress))
                {
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(DeviceInfo.IpAddress, DeviceInfo.Port ?? 502).ConfigureAwait(false);
                    _master = factory.CreateMaster(_tcpClient);
                }
                else if (!string.IsNullOrWhiteSpace(DeviceInfo.SerialPort))
                {
                    _serialPort = new SerialPort(DeviceInfo.SerialPort)
                    {
                        BaudRate = 9600,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One
                    };
                    _serialPort.Open();
                    var streamResource = new SerialPortAdapter(_serialPort);
                    _master = factory.CreateRtuMaster(streamResource);
                    _master.Transport.ReadTimeout = 2000;
                    _master.Transport.WriteTimeout = 2000;
                }
                else
                {
                    return ActivateSimulation("未配置通信参数，直接启用模拟模式。");
                }

                await _master.ReadHoldingRegistersAsync(DeviceInfo.SlaveId, 0, 1).ConfigureAwait(false);
                _useSimulation = false;
                _isConnected = true;
                DeviceInfo.Status = "Running";
                _stateContext.RaiseLog("设备连接成功，进入实时采集。");
                return true;
            }
            catch (Exception ex)
            {
                _stateContext.RaiseLog($"连接失败：{ex.Message}");
                return ActivateSimulation("通信失败，切换至模拟模式。");
            }
        }

        /// <summary>
        /// 读取一次实时数据并转换为快照；若失败则返回故障快照。
        /// </summary>
        public async Task<DeviceTelemetrySnapshot?> ReadDataAsync()
        {
            if (!_isConnected)
            {
                var success = await ConnectAsync().ConfigureAwait(false);
                if (!success)
                {
                    return BuildFaultSnapshot();
                }
            }

            try
            {
                DeviceTelemetrySnapshot snapshot;
                if (_useSimulation)
                {
                    snapshot = GenerateSimulatedSnapshot();
                }
                else
                {
                    var data = await _master!.ReadHoldingRegistersAsync(DeviceInfo.SlaveId, 0, 4).ConfigureAwait(false);
                    var status = data[0] switch
                    {
                        1 => "Running",
                        2 => "Fault",
                        _ => "Stopped"
                    };
                    var temperature = data[1] / 10d;
                    var pressure = data[2] / 10d;
                    var speed = data[3];
                    var now = DateTime.Now;
                    snapshot = new DeviceTelemetrySnapshot(DeviceInfo.DeviceId, DeviceInfo.DeviceName, status,
                        temperature, pressure, speed, now);
                }

                DeviceInfo.Status = snapshot.Status;
                DeviceInfo.LastUpdateTime = snapshot.LastUpdateTime;
                _stateContext.ApplySnapshot(snapshot);
                return snapshot;
            }
            catch (Exception ex)
            {
                _stateContext.RaiseLog($"读取失败：{ex.Message}");
                _isConnected = false;
                if (!_useSimulation)
                {
                    ActivateSimulation("读取异常，切换模拟模式。");
                    var snapshot = GenerateSimulatedSnapshot();
                    _stateContext.ApplySnapshot(snapshot);
                    return snapshot;
                }

                DeviceInfo.Status = "Fault";
                DeviceInfo.LastUpdateTime = DateTime.Now;
                return BuildFaultSnapshot();
            }
        }

        private DeviceTelemetrySnapshot BuildFaultSnapshot()
        {
            var now = DateTime.Now;
            return new DeviceTelemetrySnapshot(DeviceInfo.DeviceId, DeviceInfo.DeviceName, 
                string.IsNullOrWhiteSpace(DeviceInfo.Status) ? "Fault" : DeviceInfo.Status,
                0, 0, 0, now);
        }

        private bool ActivateSimulation(string reason)
        {
            _useSimulation = true;
            _isConnected = true;
            DeviceInfo.Status = "Running";
            _stateContext.RaiseLog(reason);
            return true;
        }

        private DeviceTelemetrySnapshot GenerateSimulatedSnapshot()
        {
            var now = DateTime.Now;
            _simTemperature = Math.Clamp(_simTemperature + NextDelta(1.8), 22d, 110d);
            _simPressure = Math.Clamp(_simPressure + NextDelta(0.4), 1.5d, 20d);
            _simSpeed = Math.Clamp(_simSpeed + (int)Math.Round(NextDelta(120)), 0, 1800);

            var statusRoll = _random.NextDouble();
            _simStatus = statusRoll switch
            {
                < 0.08 => "Fault",
                < 0.18 => "Stopped",
                _ => "Running"
            };

            return new DeviceTelemetrySnapshot(DeviceInfo.DeviceId, DeviceInfo.DeviceName, _simStatus,
                Math.Round(_simTemperature, 1), Math.Round(_simPressure, 1), _simSpeed, now);
        }

        private double NextDelta(double maxStep)
        {
            return (_random.NextDouble() * 2 - 1) * maxStep;
        }


        /// <summary>
        /// 关闭连接
        /// </summary>
        public void DisposeConnections()
        {
            _tcpClient?.Close();
            _serialPort?.Close();
            _master?.Dispose();
            _tcpClient = null;
            _serialPort = null;
            _master = null;
            _isConnected = false;
        }

        public void Dispose()
        {
            DisposeConnections();
            _isConnected = false;
        }
    }
}
