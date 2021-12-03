using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class VariationPoint
    {
        public int PercentRange { get; }
        public double Value { get; }
        public bool Resume { get; }

        const int CalcPrecision = 4;

        public VariationPoint(int percentRange, ICheckPoint checkPointUpward, ICheckPoint checkPointTopdown, double rangeMin, double rangeMax, double classPrecision, 
            double marginCoefficient = 0.8F)
        {
            PercentRange = percentRange;
            PressureUnitsEnum units = checkPointUpward.PressureUnits;

            // Вычисление вариации
            if (checkPointUpward is CurrentCheckPoint && checkPointTopdown is CurrentCheckPoint)
            {
                const double СurrentMin = 4;
                const double СurrentMax = 20;
                double currentUpwards = (checkPointUpward as CurrentCheckPoint).MeasuredCurrent;
                double currentTopdown = (checkPointTopdown as CurrentCheckPoint).MeasuredCurrent;
                Value = Math.Round(Math.Abs(100 * (currentUpwards - currentTopdown) / (СurrentMax - СurrentMin)), CalcPrecision, MidpointRounding.AwayFromZero);
            }
            else
            {
                Pressure pressureMax = new Pressure(rangeMax, units);
                Pressure pressureMin = new Pressure(rangeMin, units);
                Value = Math.Round(100 * (checkPointUpward.Pressure.PressureByUnits - checkPointTopdown.Pressure.PressureByUnits) / (pressureMax.PressureByUnits - pressureMin.PressureByUnits),
                    CalcPrecision, MidpointRounding.AwayFromZero);
            }

            // Тест в точке пройден, если вариация меньше половины класса точности с учетом коэффициента запаса
            Resume = Math.Abs(Value) < classPrecision * marginCoefficient / 2;
        }
    }
}
