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

        public ModbusDeviceWrapper(Models.Device device)
        {
            DeviceInfo = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// 连接设备 (自动识别 TCP 或 RTU)
        /// </summary>
        /// <returns>返回连接是否成功</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                //首先断开连接
                DisposeConnections();

                var factory = new ModbusFactory();
                if (!string.IsNullOrWhiteSpace(DeviceInfo.IpAddress))
                {
                    //Modbus TCP
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(DeviceInfo.IpAddress, DeviceInfo.Port ?? 502).ConfigureAwait(false);
                    _master = factory.CreateMaster(_tcpClient);
                }
                else if (!string.IsNullOrWhiteSpace(DeviceInfo.SerialPort))
                {
                    // Modbus RTU
                    _serialPort = new SerialPort(DeviceInfo.SerialPort)
                    {
                        BaudRate = 9600,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One
                    };
                    _serialPort.Open();
                    //创建RTU Master
                    var streamResource = new SerialPortAdapter(_serialPort);
                    _master = factory.CreateRtuMaster(streamResource);
                    //设置读取超时时间
                    _master.Transport.ReadTimeout = 2000;
                    _master.Transport.WriteTimeout = 2000;
                }
                else
                {
                    throw new Exception("设备未配置IP或串口号");
                }
                // 简单的连接测试：尝试读取保持寄存器地址 0 的 1 个数据
                await _master.ReadHoldingRegistersAsync(DeviceInfo.SlaveId, 0, 1).ConfigureAwait(false);
                _isConnected = true;
                DeviceInfo.Status = "Running"; // 连接成功暂且认为在运行
                return true;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DeviceInfo.DeviceName}]连接失败：{ex.Message}");
                _isConnected = false;
                DeviceInfo.Status = "Fault";//连接不上视为故障
                DeviceInfo.LastUpdateTime = DateTime.Now;
                return false;
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
                return BuildFaultSnapshot();
            }

            try
            {
                // --- 读取模拟数据 ---
                // 假设 Modbus 地址映射如下 (与 Modbus Slave 设置对应):
                // 40001 (地址0): 状态 (1=Running, 0=Stopped, 2=Fault)
                // 40002 (地址1): 温度 (放大10倍传输，如 255 代表 25.5度)
                // 40003 (地址2): 压力 (放大10倍)
                // 40004 (地址3): 转速
                // 40001 状态、40002 温度(×10)、40003 压力(×10)、40004 转速
                var data = await _master!.ReadHoldingRegistersAsync(DeviceInfo.SlaveId, 0, 4).ConfigureAwait(false);

                // 解析数据
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

                DeviceInfo.Status = status;
                DeviceInfo.LastUpdateTime = now;

                return new DeviceTelemetrySnapshot(DeviceInfo.DeviceId, DeviceInfo.DeviceName, status, temperature,
                    pressure, speed, now);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[]{DeviceInfo.DeviceName}读取失败：{ex.Message}");
                _isConnected = false;
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
            _serialPort = null;
            _isConnected = false;
        }

        public void Dispose()
        {
            DisposeConnections();
            _isConnected = false;
        }
    }
}
