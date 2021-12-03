using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class RelayTestResultcs: IEnumerable
    {
        List<RelayTestPointUpDown> results = new List<RelayTestPointUpDown>();

        public int Count { get => results.Count; } 

        public IEnumerator GetEnumerator()
        {
            return results.GetEnumerator();
        }

        public RelayTestPointUpDown this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                    return results[index];
                else
                    return null;
            }
        }

        public void Add(RelayTestPointUpDown item)
        {
            results.Add(item);
        }
    }

    public class RelayTestPointUpDown
    {
        public int PercentRange { get; private set; }
        public double RangeMin_Pa { get; private set; }
        public double RangeMax_Pa { get; private set; }
        public double MarginCoefficient { get; private set; }
        public double ClassPrecision { get; private set; }
        public PressureUnitsEnum Units { get; private set; }

        public double SP_Pa { get; private set; }
        public double Hysteresis_Pa { get; private set; }

        public RelayTestPoint PointUp { get; private set; }

        public RelayTestPoint PointDown { get; private set; }

        public RelayTestPointUpDown(int percentRange, double rangeMin_Pa, double rangeMax_Pa, 
            double classPrecision, PressureUnitsEnum pressureUnits, double sp_Pa, double hys_Pa, double marginCoefficient = 1)
        {
            PercentRange = percentRange;
            RangeMin_Pa = rangeMin_Pa;
            RangeMax_Pa = rangeMax_Pa;
            ClassPrecision = classPrecision;
            Units = pressureUnits;
            MarginCoefficient = marginCoefficient;
            SP_Pa = sp_Pa;
            Hysteresis_Pa = hys_Pa;
        }

        public void AddPointUp(double switchPressure)
        {
            PointUp = CreatePoint(SP_Pa + Hysteresis_Pa, switchPressure);
        }

        public void AddPointDown(double switchPressure)
        {
            PointUp = CreatePoint(SP_Pa - Hysteresis_Pa, switchPressure);
        }

        private RelayTestPoint CreatePoint(double sp, double switchPressure)
        {
            return new RelayTestPoint(PercentRange, RangeMin_Pa, RangeMax_Pa, sp, switchPressure, ClassPrecision, Units, MarginCoefficient);
        }
    }

    public class RelayTestPoint
    {
        public double SwitchError { get; private set; }
        public int PercentRange { get; }
        public Pressure SwitchPressure { get; }
        public Pressure SP { get; }
        public bool Resume { get; }
        public PressureUnitsEnum PressureUnits { get; }

        // Положение десятичной точки
        const int Precision = 4;

        public RelayTestPoint(int percentRange, double rangeMin_Pa, double rangeMax_Pa, double sp, double switchPressure,
            double classPrecision, PressureUnitsEnum pressureUnits, double marginCoefficient = 1)
        {
            PercentRange = percentRange;
            PressureUnits = pressureUnits;
            SP = new Pressure(sp, pressureUnits);
            SwitchPressure = new Pressure(switchPressure, pressureUnits);
            CalcError(rangeMin_Pa, rangeMax_Pa);
            // Тест пройден, если погрешность меньше класса с учетом коэффициента запаса
            Resume = Math.Abs(SwitchError) < classPrecision * marginCoefficient;
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
