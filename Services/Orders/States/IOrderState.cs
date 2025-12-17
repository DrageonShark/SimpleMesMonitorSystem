using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Services.Orders.States
{
    /// <summary>
    /// 订单状态接口：约束状态下允许的操作。
    /// </summary>
    public interface IOrderState
    {
        string Name { get; }
        bool CanStart { get; }
        bool CanPause { get; }
        bool CanResume { get; }
        bool CanComplete { get; }

        /// <summary>
        /// 对指定的订单状态上下文进行异步处理。
        /// </summary>
        /// <param name="context">待处理订单状态信息的上下文</param>
        Task StartAsync(OrderStateContext context);
        Task PauseAsync(OrderStateContext context);
        Task ResumeAsync(OrderStateContext context);
        Task CompleteAsync(OrderStateContext context);
    }
}
