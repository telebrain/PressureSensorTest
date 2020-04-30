using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTest
{
    public class PressureSystemSettings
    {
        public string IP { get; set; }
        public int[] ChannelsOut { get; set; } = new int[] { 3, 4, 5 };
        public int TimeStabilisation { get; set; } = 3;
        public int MaxTimeSetPressure { get; set; } = 30;
    }
}
