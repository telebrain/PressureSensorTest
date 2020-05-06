using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressSystems
{
    // Информация о контроллере давления
    public class PressControllerInfo
    {
        public bool Enabled { get; set; }
        public string SN { get; set; }
        public double PressureLo { get; set; }
        public double PressureHi { get; set; }
        public double Precision { get; set; }
    }
}
