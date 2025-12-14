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
            if (snapshot.Temperature > context.HighTemperatureThreshold)
            {
                context.RaiseAlarm($"温度过高：{snapshot.Temperature:F1} °C");
            }
            if (snapshot.Pressure > context.HighPressureThreshold)
            {
                context.RaiseAlarm($"压力异常：{snapshot.Pressure:F1} MPa");
            }
            if (snapshot.Speed < context.LowSpeedThreshold)
            {
                context.RaiseLog($"转速偏低（{snapshot.Speed} rpm），建议检查负载或供电。");
            }
        }

        public void OnExit(DeviceStateContext context)
        {
            context.RaiseLog("离开运行状态。");
        }
    }
}
