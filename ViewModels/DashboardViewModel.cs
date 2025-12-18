using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using WPF9SimpleMesMonitorSystem.Services.DAL;
using WPF9SimpleMesMonitorSystem.Services.Orders;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    public class DashboardViewModel: ViewModelBase, IOrderBoardObserver, IDisposable
    {
        private readonly IDbService _dbService;
        private readonly IOrderBoardSubject _orderBoardSubject;
        private readonly DispatcherTimer _timer;

        public ObservableCollection<ISeries> OrderStatusSeries { get; } = new();
        public ObservableCollection<ISeries> DeviceStatusSeries { get; } = new();
        public ObservableCollection<ISeries> OrderProgressSeries { get; } = new();
        public ObservableCollection<ISeries> ParameterSeries { get; } = new();

        public Axis[] OrderXAxes { get; private set; } = { new Axis { Labels = Array.Empty<string>(), LabelsRotation = 15 } };
        public Axis[] ParameterXAxes { get; private set; } = { new Axis { Labels = Array.Empty<string>(), LabelsRotation = 20 } };

        public DashboardViewModel(IOrderBoardSubject orderBoardSubject, IDbService dbService)
        {
            PageTitle = "仪表盘";
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _orderBoardSubject = orderBoardSubject ?? throw new ArgumentNullException(nameof(orderBoardSubject));
            InitializeSeries();
            _orderBoardSubject.Subscribe(this);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += async (_, _) => await RefreshAllAsync().ConfigureAwait(false);
            _timer.Start();
            _ = RefreshAllAsync();
        }

        private void InitializeSeries()
        {
            OrderStatusSeries.Clear();
            OrderStatusSeries.Add(new PieSeries<int> { Name = "待产", Values = new[] { 0 } });
            OrderStatusSeries.Add(new PieSeries<int> { Name = "生产中", Values = new[] { 0 } });
            OrderStatusSeries.Add(new PieSeries<int> { Name = "暂停", Values = new[] { 0 } });
            OrderStatusSeries.Add(new PieSeries<int> { Name = "完工", Values = new[] { 0 } });

            DeviceStatusSeries.Clear();
            DeviceStatusSeries.Add(new PieSeries<int> { Name = "运行", Values = new[] { 0 }, Fill = new SolidColorPaint(SKColors.MediumSeaGreen) });
            DeviceStatusSeries.Add(new PieSeries<int> { Name = "停止", Values = new[] { 0 }, Fill = new SolidColorPaint(SKColors.Gray) });
            DeviceStatusSeries.Add(new PieSeries<int> { Name = "故障", Values = new[] { 0 }, Fill = new SolidColorPaint(SKColors.IndianRed) });

            OrderProgressSeries.Clear();
            ParameterSeries.Clear();
        }

        public void OnOrderBoardUpdated(OrderBoardSnapshot snapshot)
        {
            if (snapshot == null) return;
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                ApplySnapshot(snapshot);
            }
            else
            {
                dispatcher.Invoke(() => ApplySnapshot(snapshot));
            }
        }

        private void ApplySnapshot(OrderBoardSnapshot snapshot)
        {
            if (OrderStatusSeries.Count != 4)
            {
                InitializeSeries();
            }

            UpdateSeriesValue(0, snapshot.Pending);
            UpdateSeriesValue(1, snapshot.Producing);
            UpdateSeriesValue(2, snapshot.Paused);
            UpdateSeriesValue(3, snapshot.Completed);
        }

        private void UpdateSeriesValue(int index, int value)
        {
            if (index < 0 || index >= OrderStatusSeries.Count) return;
            if (OrderStatusSeries[index] is PieSeries<int> pie)
            {
                pie.Values = new[] { value };
            }
        }

        private void UpdateDeviceSeries(int running, int stopped, int fault)
        {
            if (DeviceStatusSeries.Count != 3)
            {
                InitializeSeries();
            }

            SetPie(DeviceStatusSeries, 0, running);
            SetPie(DeviceStatusSeries, 1, stopped);
            SetPie(DeviceStatusSeries, 2, fault);
        }

        private static void SetPie(IReadOnlyList<ISeries> series, int index, int value)
        {
            if (index < 0 || index >= series.Count) return;
            if (series[index] is PieSeries<int> pie)
            {
                pie.Values = new[] { value };
            }
        }

        private async Task RefreshAllAsync()
        {
            try
            {
                await Task.WhenAll(LoadDeviceStatusAsync(), LoadOrderProgressAsync(), LoadParameterTrendAsync()).ConfigureAwait(false);
            }
            catch
            {
                // ignore timer exceptions to keep loop running
            }
        }

        private async Task LoadDeviceStatusAsync()
        {
            const string sql = "SELECT Status, COUNT(1) AS Cnt FROM dbo.T_Devices GROUP BY Status";
            var rows = await _dbService.QueryAsync<(string Status, int Cnt)>(sql).ConfigureAwait(false);
            var dict = rows.ToDictionary(r => (r.Status ?? string.Empty).Trim().ToLowerInvariant(), r => r.Cnt);
            var running = dict.TryGetValue("running", out var run) ? run : 0;
            var stopped = dict.TryGetValue("stopped", out var stop) ? stop : 0;
            var fault = dict.TryGetValue("fault", out var ft) ? ft : 0;
            await RunOnUiThreadAsync(() => UpdateDeviceSeries(running, stopped, fault)).ConfigureAwait(false);
        }

        private async Task LoadOrderProgressAsync()
        {
            const string sql = @"SELECT TOP 5 OrderNo, PlanQty, CompletedQty FROM dbo.T_ProductionOrders ORDER BY CreateTime DESC";
            var rows = await _dbService.QueryAsync<(string OrderNo, int PlanQty, int CompletedQty)>(sql).ConfigureAwait(false);
            var labels = rows.Select(r => r.OrderNo).ToArray();
            var plan = rows.Select(r => Math.Max(r.PlanQty, 0)).ToArray();
            var completed = rows.Select(r => Math.Max(r.CompletedQty, 0)).ToArray();

            await RunOnUiThreadAsync(() =>
            {
                OrderXAxes = new[] { new Axis { Labels = labels, LabelsRotation = 15 } };
                OrderProgressSeries.Clear();
                OrderProgressSeries.Add(new ColumnSeries<int> { Name = "计划", Values = plan });
                OrderProgressSeries.Add(new ColumnSeries<int> { Name = "完成", Values = completed });
                OnPropertyChanged(nameof(OrderXAxes));
            }).ConfigureAwait(false);
        }

        private async Task LoadParameterTrendAsync()
        {
            const string sql = @"SELECT TOP 20 Temperature, RecordTime FROM dbo.T_ProductionRecords ORDER BY RecordTime DESC";
            var rows = await _dbService.QueryAsync<(double Temperature, DateTime RecordTime)>(sql).ConfigureAwait(false);
            var ordered = rows.OrderBy(r => r.RecordTime).ToArray();
            var temps = ordered.Select(r => r.Temperature).ToArray();
            var labels = ordered.Select(r => r.RecordTime.ToString("HH:mm:ss")).ToArray();

            await RunOnUiThreadAsync(() =>
            {
                ParameterXAxes = new[] { new Axis { Labels = labels, LabelsRotation = 20 } };
                ParameterSeries.Clear();
                ParameterSeries.Add(new LineSeries<double>
                {
                    Name = "温度",
                    GeometryStroke = new SolidColorPaint(SKColors.SteelBlue, 2),
                    Fill = null,
                    Values = temps
                });
                OnPropertyChanged(nameof(ParameterXAxes));
            }).ConfigureAwait(false);
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

        public void Dispose()
        {
            _orderBoardSubject.Unsubscribe(this);
            _timer.Stop();
        }
    }
}
