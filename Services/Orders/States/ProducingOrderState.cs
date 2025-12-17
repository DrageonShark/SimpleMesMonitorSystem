using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.Services.Orders.States
{
    internal sealed class ProducingOrderState:IOrderState
    {
        public string Name => OrderStatus.Producing.ToString();
        public bool CanStart => false;
        public bool CanPause => true;
        public bool CanResume => false;
        public bool CanComplete => true;
        public async Task StartAsync(OrderStateContext context) => await Invalid("再次开始");
        public async Task PauseAsync(OrderStateContext context)
        {
            context.Order.OrderStatus = (int)OrderStatus.Paused;
            await context.PersistBasicFieldsAsync().ConfigureAwait(false);
            context.Log("生产已暂停");
            context.TransitionTo(context.PausedState);
        }

        public async Task ResumeAsync(OrderStateContext context) => await Invalid("恢复");

        public async Task CompleteAsync(OrderStateContext context)
        {
            context.Order.OrderStatus = (int)OrderStatus.Completed;
            await context.PersistBasicFieldsAsync().ConfigureAwait(false);
            context.Log("生产完工，完成记录已保存。");
            context.TransitionTo(context.CompletedState);
        }

        private static Task Invalid(string action) =>
            Task.FromException(new InvalidOperationException($"生产中订单无需执行“{action}”。"));
    }
}
