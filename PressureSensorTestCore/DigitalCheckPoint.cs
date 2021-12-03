using System;

namespace PressureSensorTestCore
{
    // Точка при получении значения измереного давления в цифровом виде

    public class DigitalCheckPoint : ICheckPoint
    {
        // Все значения давлений в Па

        // Точка диапазона в %
        public int PercentRange { get; }

        // Образцовое значение давления
        public Pressure ReferencePressure { get; }

        // Измеренное значение давления
        public Pressure Pressure { get; }

        // Рассчитанная погрешность измерения в %
        public double ErrorMeasure { get; private set; }

        public bool Resume { get; private set; } 
        
        // Положение десятичной точки
        const int Precision = 4;

        public PressureUnitsEnum PressureUnits { get; }

        public DigitalCheckPoint(int percentRange, double rangeMin_Pa, double rangeMax_Pa, double referencePressure_Pa, double measuredPressure_Pa,
            double classPrecision, PressureUnitsEnum pressureUnits, double marginCoefficient = 0.8F)
        {
            PercentRange = percentRange;
            PressureUnits = pressureUnits;
            ReferencePressure = new Pressure(referencePressure_Pa, pressureUnits);
            Pressure = new Pressure(measuredPressure_Pa, pressureUnits);
            CalcError(rangeMin_Pa, rangeMax_Pa);
            // Тест пройден, если погрешность меньше класса с учетом коэффициента запаса
            Resume = Math.Abs(ErrorMeasure) < classPrecision * marginCoefficient;
        }

        private void CalcError(double rangeMin, double rangeMax)
        {
            Pressure pressureMax = new Pressure(rangeMax, PressureUnits);
            Pressure pressureMin = new Pressure(rangeMin, PressureUnits);
            // Расчет основной приведенной погрешности
            ErrorMeasure = Math.Round(100 * (Pressure.PressureByUnits - ReferencePressure.PressureByUnits) / 
                    (pressureMax.PressureByUnits - pressureMin.PressureByUnits), Precision, MidpointRounding.AwayFromZero);
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
