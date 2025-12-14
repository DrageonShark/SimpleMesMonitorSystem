using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Common.Telemetry
{
    /// <summary>
    /// 任意需要监控设备实时数据的对象（视图、报表、报警等）实现该接口。
    /// 就是接收数据变化，被通知数据变化就执行更新
    /// </summary>
    public interface IDeviceTelemetryObserver
    {
        /// <summary>
        /// 由通信层推送的最新快照。
        /// </summary>
        void OnTelemetryReceived(DeviceTelemetrySnapshot snapshot);
    }
}
