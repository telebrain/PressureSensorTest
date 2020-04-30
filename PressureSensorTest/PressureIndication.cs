using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTest
{
    public class PressureIndication
    {
        readonly long range;
        readonly string format = "0.0";
        readonly double multipler = 1;

        public string UnitLable { get; } = "Па";

        public PressureIndication(double range)
        {
            this.range = (long)range;
            if (this.range >= 10000000)
            {
                UnitLable = "МПа";
                multipler = 0.000001;
            }
            else if (this.range >= 10000)
            {
                UnitLable = "кПа";
                multipler = 0.001;
            }
            else
            {
                UnitLable = "Па";
                multipler = 1;
            }
            long unitRange = (long)(range * multipler);
            if (unitRange < 10)
                format = "0.0000";
            else if (unitRange < 100)
                format = "0.000";
            else
                format = "0.00";
        }       

        public string GetPressure(double pressure)
        {
            return (pressure * multipler).ToString(format);
        }

        public string GetPressureWithUnit(double pressure)
        {
            return (pressure * multipler).ToString(format) + " " + UnitLable;
        }
    }
}
