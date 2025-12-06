using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Models
{
    public class AlarmRecord
    {
        public int AlarmId { get; set; }
        public int DeviceId { get; set; }
        public string AlarmMessage { get; set; }
        public DateTime AlarmTime { get; set; }
        public bool IsAck { get; set; } // SQL bit 对应 C# bool
    }
}
