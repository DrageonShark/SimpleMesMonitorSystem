using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Common.Telemetry
{
    /// <summary>
    /// 负责分发设备数据的主体（例如 DeviceManager）实现该接口。
    /// 就是通知其他观察者，你们的数据更新了，记得更新
    /// </summary>
    public interface IDeviceTelemetrySubject
    {

        void Subscribe(IDeviceTelemetryObserver observer);

        void Unsubscribe(IDeviceTelemetryObserver observer);
    }
}
