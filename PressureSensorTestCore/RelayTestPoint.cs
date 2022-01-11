using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class RelayTestPoint
    {
        public double? SwitchError { get; private set; }
        public Pressure SwitchPressure { get; }
        public Pressure SP { get; }

        public bool Resume { get; }
        public PressureUnitsEnum PressureUnits { get; }

        // Положение десятичной точки
        const int Precision = 4;

        public RelayTestPoint(double rangeMin_Pa, double rangeMax_Pa, double sp, double? switchPressure,
            double classPrecision, PressureUnitsEnum pressureUnits, double marginCoefficient = 1)
        {
            PressureUnits = pressureUnits;
            SP = new Pressure(sp, pressureUnits);
            if (switchPressure != null)
            {
                SwitchPressure = new Pressure(switchPressure.Value, pressureUnits);
                CalcError(rangeMin_Pa, rangeMax_Pa);
                // Тест пройден, если погрешность меньше класса с учетом коэффициента запаса
                Resume = Math.Abs(SwitchError.Value) < classPrecision * marginCoefficient;
            }
            else
            {
                SwitchPressure = null;
                SwitchError = null;
                Resume = false;
            }
           
        }

        private void CalcError(double rangeMin_Pa, double rangeMax_Pa)
        {
            Pressure pressureMax = new Pressure(rangeMax_Pa, PressureUnits);
            Pressure pressureMin = new Pressure(rangeMin_Pa, PressureUnits);
            // Расчет основной приведенной погрешности
            SwitchError = Math.Round(100 * (SP.PressureByUnits - SwitchPressure.PressureByUnits) /
                    (pressureMax.PressureByUnits - pressureMin.PressureByUnits), Precision, MidpointRounding.AwayFromZero);
        }
    }
}
