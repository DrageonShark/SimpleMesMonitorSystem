using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;

namespace WPF9SimpleMesMonitorSystem.Services.Device.States
{
    /// <summary>
    /// 设备状态接口：约束进入/离开状态时的行为。
    /// </summary>
    public interface IDeviceState
    {
        string Name { get; }

        void OnEnter(DeviceStateContext context, DeviceTelemetrySnapshot snapshot);
        void OnTelemetry(DeviceStateContext context, DeviceTelemetrySnapshot snapshot);
        void OnExit(DeviceStateContext context);
    }
}
