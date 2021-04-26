using System;

namespace PressureSensorTestCore
{
    public class CheckPoint
    {
        // Все значения давлений в Па

        // Точка диапазона в %
        public int PercentRange { get; }

        // Образцовое значение давления
        public double ReferencePressure { get; }
        
        // Измеренный ток в мА
        public double MeasuredCurrent { get; }

        // Ток, рассчитанный по образцовому значению давления
        public double CurrentFromEtalonPressure { get; private set; }

        // Давление, пересчитанное из тока изделия
        public double Pressure { get; private set; }

        // Рассчитанная погрешность измерения в %
        public double ErrorMeasure { get; private set; }

        public bool Resume { get; private set; } 
        // Положение десятичной точки
        const int Precision = 4;

        public CheckPoint(int percentRange, double rangeMin_Pa, double rangeMax_Pa, double referencePressure_Pa, double measuredCurrent,
            double classPrecision, PressureUnitsEnum pressureUnits, double marginCoefficient = 0.8F)
        {
            PercentRange = percentRange;
            ReferencePressure = Math.Round(GetPressureByUnits(referencePressure_Pa, pressureUnits), Precision, MidpointRounding.AwayFromZero);
            MeasuredCurrent = Math.Round(measuredCurrent, Precision, MidpointRounding.AwayFromZero);
            CalcError(GetPressureByUnits(rangeMin_Pa, pressureUnits), GetPressureByUnits(rangeMax_Pa, pressureUnits));
            // Тест пройден, если погрешность меньше класса с учетом коэффициента запаса
            Resume = Math.Abs(ErrorMeasure) < classPrecision * marginCoefficient;
        }

        private void CalcError(double rangeMin, double rangeMax)
        {
            const double СurrentMin = 4;
            const double СurrentMax = 20;

            // Расчет тока, соответсвующего образцовому давлению
            CurrentFromEtalonPressure = Math.Round(СurrentMin + ((СurrentMax - СurrentMin) * (ReferencePressure - rangeMin) /
                    (rangeMax - rangeMin)), Precision, MidpointRounding.AwayFromZero);

            // Считается для справки. Далее не участвует в разбраковке
            Pressure = Math.Round(((MeasuredCurrent - СurrentMin) * (rangeMax - rangeMin) / (СurrentMax - СurrentMin)) + rangeMin,
                Precision, MidpointRounding.AwayFromZero);
            // Расчет основной приведенной погрешности
            ErrorMeasure = Math.Round(100 * (MeasuredCurrent - CurrentFromEtalonPressure) / (СurrentMax - СurrentMin), 
                Precision, MidpointRounding.AwayFromZero);
        }
        
        public static double GetPressureByUnits(double val, PressureUnitsEnum pressureUnits)
        {
            double result;
            switch (pressureUnits)
            {
                case PressureUnitsEnum.MPa:
                    result = val * 1e-6;
                    break;
                case PressureUnitsEnum.KPa:
                    result = val * 1e-3;
                    break;
                default:
                    result = val;
                    break;
            }
            return Math.Round(result, Precision, MidpointRounding.AwayFromZero);
        }
    }
}
