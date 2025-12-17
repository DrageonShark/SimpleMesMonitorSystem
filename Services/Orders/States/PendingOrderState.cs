using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.Services.Orders.States
{
    internal sealed class PendingOrderState:IOrderState
    {
        public string Name => OrderStatus.Pending.ToString();
        public bool CanStart => true;
        public bool CanPause => false;
        public bool CanResume => false;
        public bool CanComplete => false;
        public async Task StartAsync(OrderStateContext context)
        {
            context.Order.OrderStatus = (int)OrderStatus.Producing;
            context.Order.StartTime ??= DateTime.Now;
            await context.PersistBasicFieldsAsync().ConfigureAwait(false);
            context.Log("已经开始生产。");
            context.TransitionTo(context.ProducingState);
        }

        public async Task PauseAsync(OrderStateContext context) => await Invalid("暂停");

        public async Task ResumeAsync(OrderStateContext context) => await Invalid("恢复");

        public async Task CompleteAsync(OrderStateContext context) => await Invalid("完工");

        private static Task Invalid(string action) =>
            Task.FromException(new InvalidOperationException($"待产订单不能执行“{{action}}”操作。"));
    }
}
