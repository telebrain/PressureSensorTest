using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class TestResults
    {
        public double RangeMin { get; }
        public double RangeMax { get; }

        public double ClassPrecision { get; }
        public double MarginCoefficient { get; }


        public MeasureResults MeasureResultsUpwards { get; private set; }
        public MeasureResults MeasureResultsTopdown { get; private set; }
        public Variations Variations { get; private set; }

        public PressureUnitsEnum PressureUnits { get;}

        public TestResults(double rangeMin, double rangeMax, PressureUnitsEnum pressureUnits, double classPrecision, double marginCoefficient = 0.8)
        {
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            PressureUnits = pressureUnits;
            ClassPrecision = classPrecision;
            MarginCoefficient = marginCoefficient;
            MeasureResultsUpwards = new MeasureResults();
            MeasureResultsTopdown = new MeasureResults();
        }

        public void CalcVariations()
        {
            Variations = null;
            if (MeasureResultsTopdown.Count != MeasureResultsUpwards.Count)
                return;

            Variations = new Variations();
            int points = MeasureResultsUpwards.Count;
            for (int i = 0; i < points; i++)
            {
                int percent = MeasureResultsUpwards[i].PercentRange;

                // if (percent == 0 || percent == 100)
                //     continue; // Для точек 0% и 100% вариация не вычисляется

                double currentUp = MeasureResultsUpwards[i].MeasuredCurrent;
                double currentDown = (MeasureResultsTopdown.GetCheckPointByPercent(percent)).MeasuredCurrent;

                var point = new VariationPoint(percent, currentUp, currentDown, ClassPrecision, MarginCoefficient);
                Variations.Add(point);               
            }
        }

        public TestResultEnum GetResume()
        {
            CalcVariations();
            if ((MeasureResultsUpwards.GetResume() & MeasureResultsTopdown.GetResume()) != true)
                return TestResultEnum.BadPrecision;
            //if (Variations.GetResume() != true)
            //    return TestResultEnum.BadVariation;
            return TestResultEnum.IsGood;
        }
    }

    public enum TestResultEnum
    {
        IsGood = 0,
        BadPrecision = 14,
        BadVariation = 19
    }

    public enum PressureUnitsEnum { Pa = 0, KPa = 1, MPa = 2 }
}
