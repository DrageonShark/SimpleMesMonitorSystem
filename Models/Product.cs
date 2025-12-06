using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF9SimpleMesMonitorSystem.Models
{
    public class Product
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public double SetTemperature { get; set; } // SQL float 对应 double
        public double SetPressure { get; set; }
        public string Description { get; set; }
    }
}
