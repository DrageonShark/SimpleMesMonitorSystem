using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;
using WPF9SimpleMesMonitorSystem.Services.DAL;

namespace WPF9SimpleMesMonitorSystem.Services.Device
{
    /// <summary>
    /// 统一调度所有 Modbus 包装器，负责轮询并向观察者广播快照。
    /// </summary>
    public class DeviceManager:IDeviceTelemetrySubject
    {
        // 单例模式
        //private static readonly Lazy<DeviceManager> _instance = new Lazy<DeviceManager>(new DeviceManager());
        //public static DeviceManager Instance => _instance.Value;

        private readonly IDbService _dbService;
        private readonly List<ModbusDeviceWrapper> _deviceWrappers = new ();
        private readonly List<IDeviceTelemetryObserver> _observers = new();
        private readonly object _observerLock = new();

        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        // 设备数据变化触发事件
        public event EventHandler<DeviceDataEventArgs> DeviceDataUpdated;
        public event EventHandler<DeviceNotificationEventArgs>? DeviceLogRaised;
        public event EventHandler<DeviceNotificationEventArgs>? DeviceAlarmRaised;

        public DeviceManager(IDbService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        /// <summary>
        /// 设备数据初始化，从数据库加载设备列表
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            //停止之前的任务
            Stop();

            const string sql = @"SELECT DeviceId, DeviceName, IpAddress, Port, SerialPort, SlaveId, Status, LastUpdateTime
                                 FROM dbo.T_Devices ORDER BY DeviceId";
            var devices = await _dbService.QueryAsync<Models.Device>(sql).ConfigureAwait(false);

            _deviceWrappers.Clear();
            foreach (var device in devices)
            {
                _deviceWrappers.Add(new ModbusDeviceWrapper(device,
                    message => RaiseDeviceLog(device, message),
                    message => RaiseDeviceAlarm(device, message)));
            }
        }

        public void Start()
        {
            if(_isRunning) return;

            _isRunning = true;
            _cts = new CancellationTokenSource();

            // 开启后台任务进行轮询
            //Task.Run(async () => await PollingLoop(_cts.Token));
            _ = Task.Run(() => PollingLoop(_cts.Token));
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

        public void Subscribe(IDeviceTelemetryObserver observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            lock (_observers)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }
        }

        public void Unsubscribe(IDeviceTelemetryObserver observer)
        {
            if (observer == null)
                return;

            lock (_observerLock)
            {
                _observers.Remove(observer);
            }
        }

        private async Task PollingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _isRunning)
            {
                //遍历所有设备进行读取
                foreach (var wrapper in _deviceWrappers)
                {
                    if(token.IsCancellationRequested) break;

                    //读取数据
                    var snapshot = await wrapper.ReadDataAsync().ConfigureAwait(false);

                    //触发事件通知UI更新
                    //使用线程安全调用
                    //DeviceDataUpdated?.Invoke(this, new DeviceDataEventArgs(wrapper.DeviceInfo));
                    if (snapshot != null) NotifyObservers(snapshot);
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private void NotifyObservers(DeviceTelemetrySnapshot snapshot)
        {
            IDeviceTelemetryObserver[] observers;
            lock (_observerLock)
            {
                observers = _observers.ToArray();
            }
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnTelemetryReceived(snapshot);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"观察者处理快照失败：{ex.Message}");
                }
            }
        }

        private void RaiseDeviceLog(Models.Device device, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            DeviceLogRaised?.Invoke(this,
                new DeviceNotificationEventArgs(device.DeviceId, device.DeviceName,
                    DeviceNotificationType.Log, message, DateTime.Now));
        }

        private void RaiseDeviceAlarm(Models.Device device, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            DeviceAlarmRaised?.Invoke(this,
                new DeviceNotificationEventArgs(device.DeviceId, device.DeviceName,
                    DeviceNotificationType.Alarm, message, DateTime.Now));
        }
    }
}
