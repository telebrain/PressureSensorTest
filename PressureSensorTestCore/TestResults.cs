﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class TestResults
    {
        public TestResults(double rangeMin, double rangeMax, double classPrecision, double marginCoefficient = 0.8F)
        {
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            ClassPrecision = classPrecision;
            MarginCoefficient = marginCoefficient;
            MeasureResultsUpwards = new MeasureResults(rangeMin, rangeMax, classPrecision, marginCoefficient);
            MeasureResultsTopdown = new MeasureResults(rangeMin, rangeMax, classPrecision, marginCoefficient);
        }

        

        public double RangeMin { get; }
        public double RangeMax { get; }

        public double ClassPrecision { get; }
        public double MarginCoefficient { get; }

        public MeasureResults MeasureResultsUpwards { get; private set; }
        public MeasureResults MeasureResultsTopdown { get; private set; }
        public Variations Variations { get; private set; }

        const double CurrentNormalizeValue = 16;

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
                double currentUp = (double)MeasureResultsUpwards[i].MeasuredCurrent;
                double currentDown = (double)(MeasureResultsTopdown.GetCheckPointByPercent(percent)).MeasuredCurrent;

                var point = new VariationPoint(percent, currentUp, currentDown, ClassPrecision, MarginCoefficient);
                Variations.Add(point);               
            }
        }

        public bool? GetResume()
        {
            CalcVariations();
            return Variations.Resume & MeasureResultsUpwards.GetResume() & MeasureResultsTopdown.GetResume();
        }
    }
}
