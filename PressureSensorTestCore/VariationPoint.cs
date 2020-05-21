using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public struct VariationPoint
    {
        public int PercentPoint { get; }
        public double Value { get; }
        public bool Resume { get; }

        const double NotmalizeCurrentValue = 16;

        //public VariationPoint(int point, double value, bool resume)
        //{
        //    Value = value;
        //    Resume = resume;
        //    Point = point;
        //}

        public VariationPoint(int percentPoint, double currentUpwards, double currentTopdown, double classPrecision, double marginCoefficient = 0.8F)
        {
            PercentPoint = percentPoint;
            Value = Math.Round(Math.Abs(100 * (currentUpwards - currentTopdown) / NotmalizeCurrentValue), 3);
            Resume = Value < classPrecision * marginCoefficient / 2;
        }
    }
}
