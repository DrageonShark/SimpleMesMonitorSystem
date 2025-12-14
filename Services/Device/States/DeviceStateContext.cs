using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Services.Device.States
{
    /// <summary>
    /// 状态上下文：负责在运行/停止/故障之间切换，并统一输出日志与报警。
    /// </summary>
    public sealed class DeviceStateContext
    {
        private readonly IDeviceState _runningState = new RunningDeviceState();
    }
}
