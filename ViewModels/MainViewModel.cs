using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    /// <summary>
    /// 主界面 VM：负责系统时钟与导航逻辑，所有页面通过 DI 解析并缓存。
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, ViewModelBase> _viewCache = new();

        //用于控制当前显示的视图（UserControl）
        [ObservableProperty] private ViewModelBase _currentView;
        //系统时间
        [ObservableProperty] private string _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //导航命令
        public IRelayCommand NavigateCommand { get; }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            NavigateCommand = new RelayCommand<string>(OnNavigate);
            //初始化时间定时器
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();
            // 初始化导航命令
            CurrentView = ResolveViewModel<DashboardViewModel>();
        }

        private void OnNavigate(string destination)
        {
            CurrentView = destination switch
            {
                "Dashboard" => new DashboardViewModel(),// 每次创建新实例，或者使用单例缓存
                "DeviceMonitor" => new DeviceMonitorViewModel(),
                "Order" => new OrderViewModel(),
                _ => CurrentView
            };
        }

        private T ResolveViewModel<T>() where T : ViewModelBase
        {
            var key = typeof(T).FullName!;
            if (!_viewCache.TryGetValue(key, out var vm))
            {
                // 缓存未命中：从DI容器获取实例
                vm = (ViewModelBase)_serviceProvider.GetRequiredService(typeof(T));
                // 存入缓存，下次复用
                _viewCache[key] = vm;
            }

            return (T)vm;
        }
    }
}
