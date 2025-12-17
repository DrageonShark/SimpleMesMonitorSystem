using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.Services.Orders.States
{
    internal sealed class PausedOrderState:IOrderState
    {
        public string Name => OrderStatus.Paused.ToString();
        public bool CanStart => false;
        public bool CanPause => false;
        public bool CanResume => true;
        public bool CanComplete => true;
        public async Task StartAsync(OrderStateContext context) => await Invalid("重新开始");

        public async Task PauseAsync(OrderStateContext context) =>
            await Task.FromException(new InvalidOperationException("订单已处于暂停状态。"));

        public async Task ResumeAsync(OrderStateContext context)
        {
            context.Order.OrderStatus = (int)OrderStatus.Producing;
            await context.PersistBasicFieldsAsync().ConfigureAwait(false);
            context.Log("订单恢复生产");
            context.TransitionTo(context.ProducingState);
        }

        public async Task CompleteAsync(OrderStateContext context)
        {
            context.Order.OrderStatus = (int)OrderStatus.Completed;
            context.Order.EndTime = DateTime.Now;
            await context.PersistBasicFieldsAsync().ConfigureAwait(false);
            context.Log("暂停状态下强制完工。");
            context.TransitionTo(context.CompletedState);
        }

        private static Task Invalid(string action) =>
            Task.FromException(new InvalidOperationException($"暂停状态无法执行“{action}”。"));
    }
}
