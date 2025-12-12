using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Common.Telemetry
{
    /// <summary>
    /// 统一封装一次轮询得到的设备实时数据快照，便于前后台模块共享。
    /// </summary>
    public sealed class DeviceTelemetrySnapshot
    {
        public DeviceTelemetrySnapshot(int deviceId, string deviceName, string status, double temperature, double pressure, int speed, DateTime lastUpdateTime)
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
            Status = status;
            Temperature = temperature;
            Pressure = pressure;
            Speed = speed;
            LastUpdateTime = lastUpdateTime;
        }
        public int DeviceId { get; }
        public string DeviceName { get; }
        public string Status { get; }
        public double Temperature { get; }
        public double Pressure { get; }
        public int Speed { get; }
        public DateTime LastUpdateTime { get; }
    }
}
