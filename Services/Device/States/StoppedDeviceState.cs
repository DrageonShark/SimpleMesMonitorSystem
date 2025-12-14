using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;

namespace WPF9SimpleMesMonitorSystem.Services.Device.States
{
    internal class StoppedDeviceState:IDeviceState
    {
        public string Name => "Stopped";
        public void OnEnter(DeviceStateContext context, DeviceTelemetrySnapshot snapshot)
        {
            context.RaiseLog("设备已停止，进入待机/维护阶段。");
        }

        public void OnTelemetry(DeviceStateContext context, DeviceTelemetrySnapshot snapshot)
        {
            if (snapshot.Speed > 0)
            {
                context.RaiseLog("检测到停止状态下仍有转速，确认是否存在惰性运行。");
            }
        }

        public void OnExit(DeviceStateContext context)
        {
            context.RaiseLog("设备即将离开停止状态。");
        }
    }
}
