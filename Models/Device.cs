using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public int? Port { get; set; }      // 允许为空，对应数据库
        [NotMapped]
        public string SerialPort { get; set; }
        public byte SlaveId { get; set; }
        public string Status { get; set; }

        public DateTime LastUpdateTime { get; set; }
        [NotMapped]
        public double CurrentTemperature { get; set; }

        // 实时压力
        [NotMapped]
        public double CurrentPressure { get; set; }

        // 产量计数 (读取到的PLC寄存器值)
        [NotMapped]
        public int CurrentCount { get; set; }
    }
}
