using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Services.DAL;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    public class DeviceManager
    {
        // 单例模式
        private static readonly Lazy<DeviceManager> _instance = new Lazy<DeviceManager>(new DeviceManager());
        public static DeviceManager Instance => _instance.Value;
        private readonly List<ModbusDeviceWrapper> _deviceWrappers = new List<ModbusDeviceWrapper>();
        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        // 设备数据变化触发事件
        public event EventHandler<DeviceDataEventArgs> DeviceDataUpdated;

        private DeviceManager(){}

        //设备数据初始化，从数据库加载设备列表
        public async Task InitializeAsync(IDbService dbService)
        {
            //停止之前的任务
            Stop();

            string sql = "SELECT * FROM T_Devices";

            var devices = await dbService.QueryAsync<Models.Device>(sql);

            _deviceWrappers.Clear();
            foreach (var device in devices)
            {
                _deviceWrappers.Add(new ModbusDeviceWrapper(device));
            }
        }

        public void Start()
        {
            if(_isRunning) return;
            _isRunning = true;
            _cts = new CancellationTokenSource();

            // 开启后台任务进行轮询
            Task.Run(async () => await PollingLoop(_cts.Token));
        }

        private void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();

            foreach (var wrapper in _deviceWrappers)
            {
                wrapper.Dispose();
            }
        }

        private async Task PollingLoop(CancellationToken token)
        {
            while (_isRunning && !_cts.IsCancellationRequested)
            {
                //遍历所有设备进行读取
                foreach (var wrapper in _deviceWrappers)
                {
                    if(token.IsCancellationRequested) break;

                    //读取数据
                    await wrapper.ReadDataAsync();

                    //触发事件通知UI更新
                    //使用线程安全调用
                    DeviceDataUpdated?.Invoke(this, new DeviceDataEventArgs(wrapper.DeviceInfo));
                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }
    }
}
