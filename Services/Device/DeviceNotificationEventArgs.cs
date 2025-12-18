using System;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    /// <summary>
    /// 标准化设备日志/报警推送的事件数据。
    /// </summary>
    public sealed class DeviceNotificationEventArgs : EventArgs
    {
        public DeviceNotificationEventArgs(int deviceId, string deviceName, DeviceNotificationType type, string message, DateTime timestamp)
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
            Type = type;
            Message = message;
            Timestamp = timestamp;
        }

        public int DeviceId { get; }
        public string DeviceName { get; }
        public DeviceNotificationType Type { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
    }

    public enum DeviceNotificationType
    {
        Log,
        Alarm
    }
}
