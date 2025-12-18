using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;
using WPF9SimpleMesMonitorSystem.Services.DAL;

namespace WPF9SimpleMesMonitorSystem.Services.Orders.States
{
    /// <summary>
    /// 订单状态上下文：协调状态流转与数据库持久化。
    /// </summary>
    public sealed class OrderStateContext
    {
        private readonly IDbService _dbService;
        private readonly Action<string> _logAction;
        private readonly IOrderState _pendingState = new PendingOrderState();
        private readonly IOrderState _producingState = new ProducingOrderState();
        private readonly IOrderState _pausedState = new PausedOrderState();
        private readonly IOrderState _completedState = new CompletedOrderState();
        private IOrderState _currentState;


        public OrderStateContext(ProductionOrder order, IDbService dbService, Action<string>? logAction = null)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _logAction = logAction ?? (_ => { });
            _currentState = ResolveState((OrderStatus)order.OrderStatus);
        }
        public ProductionOrder Order { get; }
        public IOrderState CurrentState => _currentState;
        internal IOrderState PendingState => _pendingState;
        internal IOrderState ProducingState => _producingState;
        internal IOrderState PausedState => _pausedState;
        internal IOrderState CompletedState => _completedState;

        public bool CanStart => _currentState.CanStart;
        public bool CanPause => _currentState.CanPause;
        public bool CanResume => _currentState.CanResume;
        public bool CanComplete => _currentState.CanComplete;

        private IOrderState ResolveState(OrderStatus status) => status switch
        {
            OrderStatus.Pending => _pendingState,
            OrderStatus.Producing => _producingState,
            OrderStatus.Paused => _pausedState,
            OrderStatus.Completed => _completedState,
            _ => _pendingState
        };

        public void TransitionTo(IOrderState targetState)
        {
            _currentState = targetState ?? throw new ArgumentNullException(nameof(targetState));
        }

        public void Log(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
                return;
            _logAction($"[{{Order.OrderNo}}] {{message}}");
        }

        internal Task PersistBasicFieldsAsync()
        {
            const string sql = @"UPDATE dbo.T_ProductionOrders
                                 SET OrderStatus = @OrderStatus,
                                     CompletedQty = @CompletedQty,
                                     StartTime = @StartTime,
                                     EndTime = @EndTime
                                 WHERE OrderNo = @OrderNo";
            return _dbService.ExecuteAsync(sql, new
            {
                Order.OrderStatus,
                Order.CompletedQty,
                Order.StartTime,
                Order.EndTime,
                Order.OrderNo
            });
        }

        public Task StartAsync() => _currentState.StartAsync(this);
        public Task PauseAsync() => _currentState.PauseAsync(this);
        public Task ResumeAsync() => _currentState.ResumeAsync(this);
        public Task CompleteAsync() => _currentState.CompleteAsync(this);
    }
}
