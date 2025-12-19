using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;
using WPF9SimpleMesMonitorSystem.Models;
using WPF9SimpleMesMonitorSystem.Services.DAL;
using WPF9SimpleMesMonitorSystem.Services.Device;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    /// <summary>
    /// 设备监控页面 ViewModel：负责加载设备列表并订阅实时快照。
    /// </summary>
    public partial class DeviceMonitorViewModel: ViewModelBase,IDeviceTelemetryObserver
    {
        private readonly DeviceManager _deviceManager;
        private readonly IDbService _dbService;

        //界面设备绑定
        public ObservableCollection<DeviceViewModel> Devices { get; } = new ();
        [ObservableProperty] private DeviceViewModel? _selectedDevice;

        public DeviceMonitorViewModel(DeviceManager deviceManager, IDbService dbService)
        {
            PageTitle = "设备监控";
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            
            _deviceManager.Subscribe(this);
            _ = InitializeAsync();
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

        public void OnTelemetryReceived(DeviceTelemetrySnapshot snapshot)
        {
            if(snapshot == null)
                return;
            _ = RunOnUiThreadAsync(() =>
            {
                var vm = Devices.FirstOrDefault(d => d.DeviceId == snapshot.DeviceId);
                if (vm == null)
                {
                    var model = new Device()
                    {
                        DeviceId = snapshot.DeviceId,
                        DeviceName = snapshot.DeviceName,
                        Status = snapshot.Status,
                        LastUpdateTime = snapshot.LastUpdateTime
                    };
                    vm = new DeviceViewModel(model);
                    Devices.Add(vm);
                }

                vm.ApplyTelemetry(snapshot);
            });
        }

        /// <summary>
        /// 根据数据库加载的实体创建或刷新 ViewModel 集合。
        /// </summary>
        private async Task LoadDevicesAsync()
        {
            //数据库加载设备列表
            const string sql =
                @"SELECT DeviceId, DeviceName, IpAddress, Port, SerialPort, SlaveId, Status, LastUpdateTime
                                 FROM dbo.T_Devices ORDER BY DeviceId";
            var devices = await _dbService.QueryAsync<Device>(sql).ConfigureAwait(false);

            await RunOnUiThreadAsync(() =>
            {
                Devices.Clear();
                foreach (var device in devices)
                {
                    Devices.Add(new DeviceViewModel(device));
                }
                SelectedDevice ??= Devices.FirstOrDefault();
            }).ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            await LoadDevicesAsync().ConfigureAwait(false);
            await _deviceManager.InitializeAsync().ConfigureAwait(false);
            _deviceManager.Start();
        }
        
        

        
    }
}
