using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    public class DeviceDataEventArgs:EventArgs
    {
        /// <summary>
        /// 用于在事件中传递最新的设备数据
        /// </summary>
        public Models.Device Device { get; }
        public DeviceDataEventArgs(Models.Device device)
        {
            Device = device;
        }
    }
}
