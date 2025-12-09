using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        //用于控制当前显示的视图（UserControl）
        [ObservableProperty] private ViewModelBase _currentView;
        //系统时间
        [ObservableProperty] private string _currentTime;
        //导航命令
        public IRelayCommand NavigateCommand { get; }

        public MainViewModel()
        {
            //初始化时间定时器
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();
            // 初始化导航命令
            NavigateCommand = new RelayCommand<string>(OnNavigate);
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
    }
}
