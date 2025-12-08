using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NModbus;
using NModbus.IO;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    public class ModbusDeviceWrapper:IDisposable
    {
        public Models.Device DeviceInfo { get; private set; }

        private IModbusMaster _master;
        private TcpClient _tcpClient;
        private SerialPort _serialPort;
        private bool _isConnected = false;

        /// <summary>
        /// 连接设备 (自动识别 TCP 或 RTU)
        /// </summary>
        /// <returns>返回连接是否成功</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                //首先断开连接
                Dispose();

                var factory = new ModbusFactory();
                if (!string.IsNullOrEmpty(DeviceInfo.IpAddress))
                {
                    //Modbus TCP
                    _tcpClient = new TcpClient();
                    //设置超时，防止网络不通卡死
                    await _tcpClient.ConnectAsync(DeviceInfo.IpAddress, DeviceInfo.Port ?? 502);
                    _master = factory.CreateMaster(_tcpClient);
                }
                else if (!string.IsNullOrEmpty(DeviceInfo.SerialPort))
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
                // 这里的 SlaveId 默认设为 1，Modbus Slave 模拟时要注意匹配
                byte slaveId = 1;
                await _master.ReadHoldingRegistersAsync(slaveId, 0, 1);
                _isConnected = true;
                DeviceInfo.Status = "Running"; // 连接成功暂且认为在运行
                return true;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"连接失败[{DeviceInfo.DeviceName}]:{ex.Message}");
                _isConnected = false;
                DeviceInfo.Status = "Fault";//连接不上视为故障
                return false;
            }
        }

        /// <summary>
        /// 读取数据核心逻辑
        /// </summary>
        public async Task ReadDataAsync()
        {
            if (!_isConnected)
            {
                bool success = await ConnectAsync();
                if(!success)
                    return;
            }

            try
            {
                byte slaveId = 1;
                // --- 读取模拟数据 ---
                // 假设 Modbus 地址映射如下 (与 Modbus Slave 设置对应):
                // 40001 (地址0): 状态 (1=Running, 0=Stopped, 2=Fault)
                // 40002 (地址1): 温度 (放大10倍传输，如 255 代表 25.5度)
                // 40003 (地址2): 压力 (放大10倍)
                // 40004 (地址3): 转速
                ushort[] data = await _master.ReadHoldingRegistersAsync(slaveId, 0, 4);

                // 解析数据
                ushort statusVal = data[0];
                ushort tempRaw = data[1];
                ushort pressRaw = data[2];
                ushort speedRaw = data[3];

                DeviceInfo.Status = statusVal switch
                {
                    1 => "Running",
                    2 => "Fault",
                    _ => "Stopped"
                };

                // 模拟通过 ProductionRecords 表关联的实时数据
                // 注意：这里暂时把实时数据挂在 DeviceInfo 的 Tag 上或者通过事件传出去
                // 实际项目中，Device Model 可能需要增加 Temperature 属性，或者单独传值
                // 这里为了演示，直接更新 DeviceInfo 的最后更新时间
                DeviceInfo.LastUpdateTime = DateTime.Now;

                // 为了传出实时数据，我们可以暂时封装一个对象，或者扩展 Device 类
            }
            catch (Exception ex)
            {
                _isConnected = false;
                DeviceInfo.Status = "Fault";
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Dispose()
        {
            _tcpClient?.Close();
            _serialPort?.Close();
            _master?.Dispose();
            _isConnected = false;
        }
    }
}
