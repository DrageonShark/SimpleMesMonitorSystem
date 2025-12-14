using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;

namespace WPF9SimpleMesMonitorSystem.Services.Device.States
{
    internal class RunningDeviceState:IDeviceState
    {
        public string Name => "Running";
        public void OnEnter(DeviceStateContext context, DeviceTelemetrySnapshot snapshot)
        {
            context.RaiseLog("进入运行状态，开始采集关键参数。");
        }

        public void OnTelemetry(DeviceStateContext context, DeviceTelemetrySnapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public void OnExit(DeviceStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
