using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    public interface IDisposable
    {
        /// <summary>
        /// 连接设备 (自动识别 TCP 或 RTU)
        /// </summary>
        /// <returns></returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// 读取数据核心逻辑
        /// </summary>
        /// <returns></returns>
        Task ReadDataAsync();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Dispose();
    }
}
