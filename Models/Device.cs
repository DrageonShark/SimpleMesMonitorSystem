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
        public string Status { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
