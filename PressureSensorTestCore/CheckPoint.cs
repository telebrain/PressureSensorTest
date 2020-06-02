using System;

namespace PressureSensorTestCore
{
    public class CheckPoint
    {
        // Все значения давлений в Па

        // Точка диапазона в %
        public int PercentRange { get; }

        // Образцовое значение давления
        public double EtalonPressure { get; }
        
        // Измеренный ток в мА
        public double MeasuredCurrent { get; }

        // Ток, рассчитанный по образцовому значению давления
        public double CurrentFromEtalonPressure { get; private set; }

        // Давление, пересчитанное из тока изделия
        public double Pressure { get; private set; }

        // Рассчитанная погрешность измерения в %
        public double? ErrorMeasure { get; private set; } = null;

        public bool? Resume { get; private set; } = null;
                

        public CheckPoint(int percentRange, double etalonPressure, double measuredCurrent)
        {
            PercentRange = percentRange;
            EtalonPressure = etalonPressure;
            MeasuredCurrent = measuredCurrent;
        }

        public void CalcError(double rangeMin, double rangeMax)
        {
            const double СurrentMin = 4;
            const double СurrentMax = 20;

            // Расчет тока, соответсвующего образцовому давлению
            CurrentFromEtalonPressure = СurrentMin + ((СurrentMax - СurrentMin) * (EtalonPressure - rangeMin) /
                    (rangeMax - rangeMin));

            // Считается для справки. Далее не участвует в разбраковке
            Pressure = ((MeasuredCurrent - СurrentMin) * (rangeMax - rangeMin) / (СurrentMax - СurrentMin)) + rangeMin;
            // Расчет основной приведенной погрешности
            ErrorMeasure = Math.Round(100 * (MeasuredCurrent - CurrentFromEtalonPressure) / (СurrentMax - СurrentMin), 3);
        }

        public bool? CheckResult(double classPrecision, double marginCoefficient = 0.8F)
        {
            // Если результаты еще не готовы
            if (ErrorMeasure == null) 
                return null; // Возвращается null

            // Тест пройден, если погрешность меньше класса с учетом коэффициента запаса
            Resume = Math.Abs(ErrorMeasure.Value) < classPrecision * marginCoefficient;
            return Resume;
        }
    }
}
