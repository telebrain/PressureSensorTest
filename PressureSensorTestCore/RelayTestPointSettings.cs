using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class RelayTestPointSettings
    {
        // Все давление в Па
        public double TestPoint { get; }
        public double RangeMin { get; }
        public double RangeMax { get; }
        public double ClassPrecision { get; }

        // Уставка, передается на тестируемый прибор
        public double DeviceSP { get; }
        // Гистерезис, передается на тестируемый прибор
        public double Hysteresis { get; }
        // Уставка для теста переключения при нарастании давления (тест вверх)
        public double PsysUpwardSP { get; }
        // Уставка для теста переключения при снижении давления (тест вниз)
        public double PsysDownwardSP { get; }
        // Если давление достигло этой величины при тесте вверх, а реле не переключилось тест считается законченным
        public double PressureLimitUpward { get; }
        // Если давление достигло этой величины при тесте вниз, а реле не переключилось тест считается законченным
        public double PressureLimitDownward { get; }
        public double PsysRate { get; }

        const double hysteresisCoefficient = 4;
        const double rateCoefficient = 10;

        public RelayTestPointSettings(double testPoint, double rangeMin, double rangeMax, double classPrecision)
        {
            TestPoint = testPoint;
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            ClassPrecision = classPrecision;
            double absPrecision = Math.Abs(rangeMax - rangeMin) * classPrecision / 100;
            Hysteresis = hysteresisCoefficient * absPrecision;
            DeviceSP = testPoint * (rangeMax - rangeMin) + rangeMin;
            double signDirect = rangeMax > rangeMin ? 1 : -1; // Знак направления. Для типа ДВ будет -1
            PsysRate = absPrecision / rateCoefficient;
            PsysDownwardSP = DeviceSP - 3 * Hysteresis * signDirect;
            PressureLimitDownward = DeviceSP - 2 * Hysteresis * signDirect;
            PsysUpwardSP = DeviceSP + 3 * Hysteresis * signDirect;
            PressureLimitUpward = DeviceSP + 2 * Hysteresis * signDirect;
        }
    }
}
