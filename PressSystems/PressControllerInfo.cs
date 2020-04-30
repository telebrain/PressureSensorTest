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
        public float PressureLo { get; set; }
        public float PressureHi { get; set; }
        public float Precision { get; set; }
    }
}
