using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NModbus;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    public class ModbusDeviceWrapper:IDisposable
    {
        public Models.Device DeviceInfo { get; private set; }

        private IModbusMaster _master;
        private TcpClient _tcpClient;
        private SerialPort _serialPort;

        public async Task<bool> ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public async Task ReadDataAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
