using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class RelayTestPointUpDown
    {
        public int PercentRange { get; private set; }
        public double RangeMin_Pa { get; private set; }
        public double RangeMax_Pa { get; private set; }
        public double MarginCoefficient { get; private set; }
        public double ClassPrecision { get; private set; }
        public PressureUnitsEnum Units { get; private set; }

        public Pressure SP { get; private set; }
        public Pressure Hysteresis { get; private set; }

        public RelayTestPoint RelayTestPointUp { get; private set; }

        public RelayTestPoint RelayTestPointDown { get; private set; }

        public RelayTestPointUpDown(RelayTestPointSettings settings, PressureUnitsEnum pressureUnits, double marginCoefficient = 1)
        {
            PercentRange = (int)settings.TestPoint*100;
            RangeMin_Pa = settings.RangeMin;
            RangeMax_Pa = settings.RangeMax;
            ClassPrecision = settings.ClassPrecision;
            Units = pressureUnits;
            MarginCoefficient = marginCoefficient;
            SP = new Pressure(settings.DeviceSP, pressureUnits);
            Hysteresis = new Pressure(settings.Hysteresis, pressureUnits);
        }

        public void AddPointUp(double? switchPressure)
        {
            double sp = RangeMin_Pa < RangeMax_Pa ? (SP.Pressure_Pa + Hysteresis.Pressure_Pa) :
                (SP.Pressure_Pa - Hysteresis.Pressure_Pa);

            RelayTestPointUp = new RelayTestPoint(RangeMin_Pa, RangeMax_Pa, sp, switchPressure, ClassPrecision, 
                Units, MarginCoefficient);
        }

        public void AddPointDown(double? switchPressure)
        {
            double sp = RangeMin_Pa < RangeMax_Pa ? (SP.Pressure_Pa - Hysteresis.Pressure_Pa) :
                (SP.Pressure_Pa + Hysteresis.Pressure_Pa);

            RelayTestPointUp = new RelayTestPoint(RangeMin_Pa, RangeMax_Pa, sp, switchPressure, ClassPrecision,
                Units, MarginCoefficient);
        }
    }
}
