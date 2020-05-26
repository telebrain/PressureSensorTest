using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwenPressureDevices
{
    public class DeviceRange
    {
        // Диапазон сенсора
        // Все величины давления в Па

        public double Span { get; private set; }
        public double Min { get; private set; }
        public double Max { get; private set; }
        // public string RangeTypeLabel { get; private set; }
        public RangeTypeEnum RangeType { get; private set; }
        public long Pressure_Pa { get; private set; }
        public bool AbsolutPressure { get; private set; } // Флаг датчика абсолютного давления

        public static long VacuumPressure = -100000; // Относительное давление вакуума

        public DeviceRange(int pressure_Pa, RangeTypeEnum rangeType)
        {
            CreateDevRange(pressure_Pa, rangeType);
        }

        private void CreateDevRange(long pressure_Pa, RangeTypeEnum rangeType)
        {
            Pressure_Pa = pressure_Pa;
            RangeType = rangeType;
            AbsolutPressure = false;
            switch (rangeType)
            {
                case RangeTypeEnum.DIV:
                    if (pressure_Pa > Math.Abs(VacuumPressure))
                    {
                        Min = VacuumPressure;
                        Max = pressure_Pa;
                    }
                    else
                    {
                        Min = (-1) * pressure_Pa;
                        Max = pressure_Pa;
                    }
                    break;

                case RangeTypeEnum.DI:
                    Min = 0;
                    Max = pressure_Pa;
                    break;
                case RangeTypeEnum.DD:
                    Min = 0;
                    Max = pressure_Pa;
                    break;
                case RangeTypeEnum.DV:
                    Min = 0;
                    Max = (-1) * pressure_Pa;
                    break;
                case RangeTypeEnum.DA:
                    Min = 0;
                    Max = pressure_Pa;
                    AbsolutPressure = true;
                    break;
                case RangeTypeEnum.DG:
                    Min = 0;
                    Max = pressure_Pa * 0.980665F;
                    break;
            }

            Span = Math.Abs(Max - Min);
        }

        
        //public RangeTypeEnum GetRangeTypeFromLabel(string label)
        //{
        //    return (RangeTypeEnum)Array.IndexOf(Labels, label);
        //}
    }

    public enum RangeTypeEnum : int
    {
        DI = 0,
        DV = 1,
        DIV = 2,
        DD = 3,
        DA = 4,
        DG = 5,
        NotDefined = -1
    }

}
