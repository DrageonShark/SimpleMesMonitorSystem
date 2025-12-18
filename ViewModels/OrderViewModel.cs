using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF9SimpleMesMonitorSystem.Models;
using WPF9SimpleMesMonitorSystem.Services.DAL;
using WPF9SimpleMesMonitorSystem.Services.Orders;
using WPF9SimpleMesMonitorSystem.Services.Orders.States;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    /// <summary>
    /// 订单管理页面 ViewModel：加载订单、封装状态命令与操作日志。
    /// </summary>
    public partial class OrderViewModel : ViewModelBase
    {

        public OrderViewModel(IDbService dbService, IOrderBoardSubject orderBoardSubject)
        {
            PageTitle = "订单管理";
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _orderBoardSubject = orderBoardSubject ?? throw new ArgumentNullException(nameof(orderBoardSubject));
            RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync);
            StartOrderCommand = new AsyncRelayCommand(StartAsync, () => _orderContext?.CanStart ?? false);
            PauseOrderCommand = new AsyncRelayCommand(PauseAsync, () => _orderContext?.CanPause ?? false);
            ResumeOrderCommand = new AsyncRelayCommand(ResumeAsync, () => _orderContext?.CanResume ?? false);
            CompleteOrderCommand = new AsyncRelayCommand(CompleteAsync, () => _orderContext?.CanComplete ?? false);
            _ = LoadOrdersAsync();
        }

        private readonly IDbService _dbService;
        private readonly IOrderBoardSubject _orderBoardSubject;
        private OrderStateContext? _orderContext;


        public ObservableCollection<ProductionOrder> Orders { get; } = new();
        public ObservableCollection<string> OperationLogs { get; } = new();
        [ObservableProperty] private ProductionOrder? _selectOrder;

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand StartOrderCommand { get; }
        public IAsyncRelayCommand PauseOrderCommand { get; }
        public IAsyncRelayCommand ResumeOrderCommand { get; }
        public IAsyncRelayCommand CompleteOrderCommand { get; }

        partial void OnSelectOrderChanged(ProductionOrder? value)
        {
            _orderContext = value == null ? null : new OrderStateContext(value, _dbService, AppendLog);
            RefreshCommandStates();
        }

        private void RefreshCommandStates()
        {
            StartOrderCommand.NotifyCanExecuteChanged();
            PauseOrderCommand.NotifyCanExecuteChanged();
            ResumeOrderCommand.NotifyCanExecuteChanged();
            CompleteOrderCommand.NotifyCanExecuteChanged();
        }

        private async Task LoadOrdersAsync()
        {
            const string sql =
                @"SELECT OrderNo, ProductCode, PlanQty, CompletedQty, OrderStatus, StartTime, EndTime, CreateTime
                                 FROM dbo.T_ProductionOrders ORDER BY CreateTime DESC";
            try
            {
                var orders = await _dbService.QueryAsync<ProductionOrder>(sql).ConfigureAwait(false);
                await RunOnUiThreadAsync(() =>
                {
                    Orders.Clear();
                    foreach (var order in orders)
                    {
                        Orders.Add(order);
                    }
                    if (!Orders.Contains(SelectOrder))
                    {
                        SelectOrder = Orders.FirstOrDefault();
                    }
                }).ConfigureAwait(false);
                AppendLog("订单列表刷新完成。");
                await PublishBoardSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendLog($"刷新订单列表失败：{ex.Message}");
            }
        }

        private async Task StartAsync()
        {
            if (_orderContext == null)
            {
                return;
            }

            try
            {
                await _orderContext.StartAsync().ConfigureAwait(false);
                await RefreshCurrentOrderSnapshotAsync().ConfigureAwait(false);
                await PublishBoardSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendLog($"开始订单失败：{ex.Message}");
            }
            finally
            {
                RefreshCommandStates();
            }
        }

        private async Task PauseAsync()
        {
            if (_orderContext == null)
            {
                return;
            }

            try
            {
                await _orderContext.PauseAsync().ConfigureAwait(false);
                await RefreshCurrentOrderSnapshotAsync().ConfigureAwait(false);
                await PublishBoardSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendLog($"暂停订单失败：{ex.Message}");
            }
            finally
            {
                RefreshCommandStates();
            }
        }

        private async Task ResumeAsync()
        {
            if (_orderContext == null)
            {
                return;
            }

            try
            {
                await _orderContext.ResumeAsync().ConfigureAwait(false);
                await RefreshCurrentOrderSnapshotAsync().ConfigureAwait(false);
                await PublishBoardSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendLog($"恢复订单失败：{ex.Message}");
            }
            finally
            {
                RefreshCommandStates();
            }
        }

        private async Task CompleteAsync()
        {
            if (_orderContext == null)
            {
                return;
            }

            try
            {
                await _orderContext.CompleteAsync().ConfigureAwait(false);
                await RefreshCurrentOrderSnapshotAsync().ConfigureAwait(false);
                await PublishBoardSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendLog($"完工订单失败：{ex.Message}");
            }
            finally
            {
                RefreshCommandStates();
            }
        }

        private async Task RefreshCurrentOrderSnapshotAsync()
        {
            if (SelectOrder == null)
            {
                return;
            }
            const string sql = @"SELECT OrderNo, ProductCode, PlanQty, CompletedQty, OrderStatus, StartTime, EndTime, CreateTime
                                 FROM dbo.T_ProductionOrders WHERE OrderNo = @OrderNo";
            try
            {
                var latest = await _dbService
                    .QueryFirstOrDefaultAsync<ProductionOrder>(sql, new { OrderNo = SelectOrder.OrderNo })
                    .ConfigureAwait(false);
                if (latest == null)
                {
                    return;
                }
                await RunOnUiThreadAsync(() =>
                {
                    var index = Orders.IndexOf(SelectOrder);
                    if (index >= 0)
                    {
                        Orders[index] = latest;
                        SelectOrder = latest;
                    }
                }).ConfigureAwait(false);
                await PublishBoardSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendLog($"刷新订单状态失败：{ex.Message}");
            }
        }

        private Task PublishBoardSnapshotAsync()
        {
            return RunOnUiThreadAsync(() =>
            {
                var pending = Orders.Count(o => o.OrderStatus == (int)OrderStatus.Pending);
                var producing = Orders.Count(o => o.OrderStatus == (int)OrderStatus.Producing);
                var paused = Orders.Count(o => o.OrderStatus == (int)OrderStatus.Paused);
                var completed = Orders.Count(o => o.OrderStatus == (int)OrderStatus.Completed);
                var snapshot = new OrderBoardSnapshot(pending, producing, paused, completed, DateTime.Now);
                _orderBoardSubject.Publish(snapshot);
            });
        }

        private static Task RunOnUiThreadAsync(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }

            return dispatcher.InvokeAsync(action).Task;
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            void Insert()
            {
                var entry = $"{DateTime.Now:HH:mm:ss} {message}";
                OperationLogs.Insert(0,entry);
                if (OperationLogs.Count > 100)
                {
                    OperationLogs.RemoveAt(OperationLogs.Count - 1);
                }
            }

            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                Insert();
            }
            else
            {
                dispatcher.Invoke(Insert);
            }
        }
    }
}
