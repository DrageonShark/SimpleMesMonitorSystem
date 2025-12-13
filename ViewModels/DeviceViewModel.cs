using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WPF9SimpleMesMonitorSystem.Common.Telemetry;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    /// <summary>
    /// UI 层可观察实体，封装设备基础信息，把实时快照映射为可绑定属性。
    /// </summary>
    public partial class DeviceViewModel : ViewModelBase
    {
        public Device Model { get; }

        public DeviceViewModel(Device model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            PageTitle = model.DeviceName;
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
        [ObservableProperty] private int _currentSpeed;
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
        /// 根据实时快照刷新 UI，并把关键字段写回 Model，方便后续持久化。
        /// </summary>
        public void ApplyTelemetry(DeviceTelemetrySnapshot snapshot)
        {
            if(snapshot == null) 
                throw new ArgumentNullException(nameof(snapshot));
            Model.Status = snapshot.Status;
            Model.LastUpdateTime = snapshot.LastUpdateTime;

            Status = snapshot.Status;
            LastUpdateTime = snapshot.LastUpdateTime;
            CurrentTemperature = snapshot.Temperature;
            CurrentPressure = snapshot.Pressure;
            CurrentSpeed = snapshot.Speed;
        }
    }
}
