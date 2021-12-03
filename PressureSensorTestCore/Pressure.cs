using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public struct Pressure
    {
        public double Pressure_Pa { get; }

        public double PressureByUnits { get; }

        public PressureUnitsEnum Units { get; }
 
        // Положение десятичной точки
        const int Precision = 4;

        public Pressure(double pressure_Pa)
        {
            Pressure_Pa = pressure_Pa;
            PressureByUnits = pressure_Pa;
            Units = PressureUnitsEnum.Pa;
        }

        public Pressure(double pressure_Pa, PressureUnitsEnum units)
        {
            
            Pressure_Pa = pressure_Pa;
            Units = PressureUnitsEnum.Pa;
            double multipler;
            switch (units)
            {
                case PressureUnitsEnum.MPa:
                    multipler = 1e-6;
                    break;
                case PressureUnitsEnum.KPa:
                    multipler = 1e-3;
                    break;
                default:
                    multipler = 1;
                    break;
            }
            PressureByUnits = Math.Round(Pressure_Pa * multipler, Precision, MidpointRounding.AwayFromZero);
        }
    }

    public enum PressureUnitsEnum { Pa = 0, KPa = 1, MPa = 2 }
}
