using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;

namespace WPF9SimpleMesMonitorSystem.Services.Device.States
{
    internal class FaultDeviceState:IDeviceState
    {
        public string Name => "Fault";
        public void OnEnter(DeviceStateContext context, DeviceTelemetrySnapshot snapshot)
        {
            context.RaiseAlarm("进入故障状态，请尽快派维修人员处理。");
        }

        public void OnTelemetry(DeviceStateContext context, DeviceTelemetrySnapshot snapshot)
        {
            context.RaiseLog($"故障期间采集到的数据：T={snapshot.Temperature:F1} °C, " +
                             $"P={snapshot.Pressure:F1} MPa, S={snapshot.Speed} rpm。");
        }

        public void OnExit(DeviceStateContext context)
        {
            context.RaiseLog("故障解除，准备切换到新状态。");
        }
    }
}
