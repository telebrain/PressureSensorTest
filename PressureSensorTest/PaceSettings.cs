using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTest
{
    [Serializable]
    public class PaceSettings
    {
        public string PortName { get; set; } = "COM1";

        public double SrcPlusValue { get; set; } = 10000000;

        public double SrcMinusValue { get; set; } = 0;

        public int TimeStabilisation { get; set; } = 3;

        public int MaxTimeSetPressure { get; set; } = 60;

        public string ClassPrecision { get; set; } = "0,02";
    }
}
