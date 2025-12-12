using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Common.Telemetry
{
    /// <summary>
    /// 负责分发设备数据的主体（例如 DeviceManager）实现该接口。
    /// </summary>
    public interface IDeviceTelemetrySubject
    {

        void Subscribe(IDeviceTelemetryObserver observer);

        void Unsubscribe(IDeviceTelemetryObserver observer);
    }
}
