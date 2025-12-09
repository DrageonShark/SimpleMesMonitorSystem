using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    /// <summary>
    /// UI 层可观察实体，封装设备基础信息和实时状态。
    /// </summary>
    public partial class DeviceViewModel : ViewModelBase
    {
        public Device Model { get; }

        public DeviceViewModel(Device model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _status = model.Status;
            _lastUpdateTime = model.LastUpdateTime;
        }

        public int DeviceId => Model.DeviceId;
        public string DeviceName => Model.DeviceName;
        public string IpAddress => Model.IpAddress;
        public int? Port => Model.Port;
        public string SerialPort => Model.SerialPort;
        public byte SlaveId => Model.SlaveId;

        [ObservableProperty]
        private double _currentTemperature;

        [ObservableProperty]
        private double _currentPressure;

        [ObservableProperty]
        private int _currentCount;

        [ObservableProperty]
        private string _status;

        [ObservableProperty]
        private DateTime _lastUpdateTime;

        /// <summary>
        /// 将实体中的最新持久化字段同步到可绑定属性。
        /// </summary>
        public void UpdateFromModel()
        {
            Status = Model.Status;
            LastUpdateTime = Model.LastUpdateTime;
        }

        /// <summary>
        /// 提供一个入口刷新实时量测参数，供设备轮询结果调用。
        /// </summary>
        public void ApplyRealtimeSnapshot(double temperature, double pressure, int count)
        {
            CurrentTemperature = temperature;
            CurrentPressure = pressure;
            CurrentCount = count;
        }
    }
}
