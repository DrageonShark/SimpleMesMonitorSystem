using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF9SimpleMesMonitorSystem.Models;

namespace WPF9SimpleMesMonitorSystem.ViewModels
{
    public partial class DeviceMonitorViewModel: ViewModelBase
    {
        //界面设备绑定
        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        public DeviceMonitorViewModel()
        {
            
        }
        
        private async void LoadDevices()
        {
            //数据库加载设备列表

        }

        /// <summary>
        /// 根据数据库加载的实体创建或刷新 ViewModel 集合。
        /// </summary>
        public void ApplyDeviceSnapshot(IEnumerable<Device> devices)
        {
            if (devices == null)
                return;

            foreach (var device in devices)
            {
                UpdateDevice(device);
            }
        }

        /// <summary>
        /// 根据最新实体数据刷新单台设备的 UI 状态。
        /// </summary>
        public DeviceViewModel UpdateDevice(Device device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var existing = Devices.FirstOrDefault(d => d.DeviceId == device.DeviceId);
            if (existing == null)
            {
                existing = new DeviceViewModel(device);
                Devices.Add(existing);
            }
            else
            {
                existing.UpdateFromModel();
            }

            return existing;
        }
    }
}
