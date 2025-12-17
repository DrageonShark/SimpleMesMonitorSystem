using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.Services.Orders.States
{
    internal sealed class CompletedOrderState:IOrderState
    {
        public string Name => OrderStatus.Completed.ToString();
        public bool CanStart => false;
        public bool CanPause => false;
        public bool CanResume => false;
        public bool CanComplete => false;
        public async Task StartAsync(OrderStateContext context) => await Invalid();

        public async Task PauseAsync(OrderStateContext context) => await Invalid();

        public async Task ResumeAsync(OrderStateContext context) => await Invalid();

        public async Task CompleteAsync(OrderStateContext context) => await Invalid();
        private static Task Invalid() =>
            Task.FromException(new InvalidOperationException("已完工订单不允许再修改状态。"));
    }
}
