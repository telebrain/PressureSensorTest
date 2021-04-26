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

        public VariationPoint(int percentRange, double currentUpwards, double currentTopdown, double classPrecision, double marginCoefficient = 0.8F)
        {
            PercentRange = percentRange;

            const double СurrentMin = 4;
            const double СurrentMax = 20;

            // Вычисление вариации
            Value = Math.Round(Math.Abs(100 * (currentUpwards - currentTopdown) /
            (СurrentMax - СurrentMin)), CalcPrecision, MidpointRounding.AwayFromZero);

            // Тест в точке пройден, если вариация меньше половины класса точности с учетом коэффициента запаса
            Resume = Value < classPrecision * marginCoefficient / 2;
        }
    }
}
