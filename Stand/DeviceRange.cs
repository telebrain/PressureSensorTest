using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwenPressureDevices
{
    // Кдасс хранит данные о диапазоне преобразователя давления
    public class DeviceRange
    {

        // Все величины давления в Па

        public double Span { get; private set; }
        public double Min_Pa { get; private set; }
        public double Max_Pa { get; private set; }

        public double Min { get; private set; }
        public double Max { get; private set; }

        public PressureUnitsEnum PressureUnits { get; private set; }

        public string RangeTypeLabel { get; private set; }
        public RangeTypeEnum RangeType { get; private set; }
        public long Pressure_Pa { get; private set; }
        public bool AbsolutPressure { get; private set; } // Флаг датчика абсолютного давления

        public static long VacuumPressure = -100000; // Относительное давление вакуума

        static Dictionary<string, RangeTypeEnum> rangeTypes = new Dictionary<string, RangeTypeEnum>()
        {
            { "ДИ", RangeTypeEnum.DI }, { "ДИВ", RangeTypeEnum.DIV }, { "ДВ", RangeTypeEnum.DV }, { "ДД", RangeTypeEnum.DD },
                { "ДГ", RangeTypeEnum.DG }, {"ДА", RangeTypeEnum.DA}
        };
            

        public static string GetRangeTypeLabel(RangeTypeEnum rangeType)
        {
            return (rangeTypes.FirstOrDefault(item => item.Value == rangeType)).Key;
        }

        public static RangeTypeEnum GetRangeTypeByLable(string rangeType)
        {
            return rangeTypes[rangeType];
        }

        public DeviceRange(int pressure_Pa, string rangeTypeLabel):
            this(pressure_Pa, GetRangeTypeByLable(rangeTypeLabel))
        { }


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
                        Min_Pa = VacuumPressure;
                        Max_Pa = pressure_Pa;
                    }
                    else
                    {
                        Min_Pa = (-1) * pressure_Pa;
                        Max_Pa = pressure_Pa;
                    }
                    break;

                case RangeTypeEnum.DI:
                    Min_Pa = 0;
                    Max_Pa = pressure_Pa;
                    break;
                case RangeTypeEnum.DD:
                    Min_Pa = 0;
                    Max_Pa = pressure_Pa;
                    break;
                case RangeTypeEnum.DV:
                    Min_Pa = 0;
                    Max_Pa = (-1) * pressure_Pa;
                    break;
                case RangeTypeEnum.DA:
                    Min_Pa = 0;
                    Max_Pa = pressure_Pa;
                    AbsolutPressure = true;
                    break;
                case RangeTypeEnum.DG:
                    Min_Pa = 0;
                    Max_Pa = pressure_Pa * 0.980665F;
                    break;
            }

            Span = Math.Abs(Max_Pa - Min_Pa);
            SetUnits();
        }

        public void SetUnits()
        {
            if (Pressure_Pa < 10000)
            {
                PressureUnits = PressureUnitsEnum.Pa;
                Min = Min_Pa;
                Max = Max_Pa;
            }
            else if (Pressure_Pa < 10000000)
            {
                PressureUnits = PressureUnitsEnum.kPa;
                Min = Min_Pa * 1e-3;
                Max = Max_Pa * 1e-3;
            }
            else
            {
                PressureUnits = PressureUnitsEnum.MPa;
                Min = Min_Pa * 1e-6;
                Max = Max_Pa * 1e-6;
            }

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

    // public enum PressureUnitsEnum: int { Pa = 0, KPa = 1, MPa = 2 }
}
