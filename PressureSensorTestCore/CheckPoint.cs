using System;

namespace PressureSensorTestCore
{
    public class CheckPoint
    {
        // Все значения давлений в Па

        // Точка диапазона в %
        public int PercentRange { get; }

        // Образцовое значение давления
        public double? EtalonPressure { get; } = null;

        // Ток, рассчитанный по образцовому значению давления
        public double? CurrentFromEtalonPressure { get; private set; }
        
        // Измеренный ток в мА
        public double? MeasuredCurrent { get; } = null;

        // Давление, пересчитанное из тока изделия
        public double? Pressure { get; private set; } = null;

        // Рассчитанная погрешность измерения в %
        public double? ErrorMeasure { get; private set; }

        public bool? Resume { get; private set; }
                

        public CheckPoint(int percentRange, double etalonPressure, double measuredCurrent)
        {
            PercentRange = percentRange;
            EtalonPressure = etalonPressure;
            MeasuredCurrent = measuredCurrent;
        }

        public bool? CheckResult(double classPrecision, double marginCoefficient = 0.8F)
        {
            Resume = Math.Abs((double)ErrorMeasure) < classPrecision * marginCoefficient;
            return Resume;
        }

        public void CalcError(double rangeMin, double rangeMax)
        {
            const double currentMin = 4;
            const double currentMax = 20;
            
            CurrentFromEtalonPressure = currentMin + ((currentMax - currentMin) * (EtalonPressure - rangeMin) /
                    (rangeMax - rangeMin));

            // Считается для справки. Далее не участвует в разбраковке
            Pressure = ((MeasuredCurrent - currentMin) * (rangeMax - rangeMin) / (currentMax - currentMin)) + rangeMin;

            ErrorMeasure = (double)Math.Round((double)(100 * (MeasuredCurrent - CurrentFromEtalonPressure) / (currentMax - currentMin)), 3);

        }

        //public double?[] GetResultAsArray()
        //{
        //    return new double?[] { EtalonPressure, CurrentFromEtalonPressure, MeasuredCurrent, Pressure, ErrorMeasure };
        //}
    }
}
