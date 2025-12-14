using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WPF9SimpleMesMonitorSystem.Services.DAL;
using WPF9SimpleMesMonitorSystem.Services.Device;
using WPF9SimpleMesMonitorSystem.ViewModels;

namespace WPF9SimpleMesMonitorSystem
{
    /// <summary>
    /// 应用程序入口：此处负责构建全局 ServiceProvider，并在启动时解析主窗口。
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        /// <summary>
        /// 统一的依赖注入容器（IoC），用于解析 View、ViewModel、服务等实例。
        /// </summary>
        public IServiceProvider Services { get; }

        public App()
        {
            // 构建依赖注入容器，后续所有需要的实例都从这里获取。
            Services = ConfigureService();
        }

        /// <summary>
        /// 覆写 OnStartup，主动解析 MainWindow，确保它获得注入的依赖后再显示。
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        /// <summary>
        /// 负责把所有服务、ViewModel、窗口统一注册到 DI 容器中。
        /// </summary> 
        private static IServiceProvider ConfigureService()
        {
            var services = new ServiceCollection();

            // 数据访问工厂参数，可后续改为读取 appsettings.json


            //TODO:可改为读取配置文件，这里先写死，方便快速跑通。
            const string connectionString = "server=localhost;database=MesDb;uid=sa;pwd=123456";
            // ——数据访问层（工厂模式：IDbService -> SqlDbService）——
            services.AddSingleton<IDbService>(_ => new SqlDbService(connectionString));
            //——设备通信层（单例：统一管理所有Modbus设备）——
            services.AddSingleton<DeviceManager>();
            //——ViewModel注册——
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DeviceMonitorViewModel>();
            services.AddTransient<OrderViewModel>();
            //——主窗口注册——
            //在创建MainWindow时同步注入MainViewModel作为DataContext。
            services.AddSingleton<MainWindow>(sp =>
            {
                var window = new MainWindow
                {
                    DataContext = sp.GetRequiredService<MainViewModel>()
                };
                return window;
            });
            return services.BuildServiceProvider();
        }

        
    }

}
