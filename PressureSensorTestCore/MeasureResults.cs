using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class MeasureResults: IEnumerable
    {
        // Все значения давлений в Па

        public double RangeMin { get; }
        public double RangeMax { get; }

        public double ClassPrecision { get; }
        public double MarginCoefficient { get; }

        public int Count
        {
            get { return checkPoints.Count; }
        }

        List<CheckPoint> checkPoints = new List<CheckPoint>();

        public MeasureResults(double rangeMin, double rangeMax, double classPrecision, double marginCoefficient = 0.8F)
        {
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            ClassPrecision = classPrecision;
            MarginCoefficient = marginCoefficient;
        }

        

        public CheckPoint this[int index]
        {
            get
            {
                if (index >= 0 && index < checkPoints.Count)
                {
                    return checkPoints[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public CheckPoint GetCheckPointByPercent(int percentRange)
        {
            foreach(var point in checkPoints)
            {
                if (point.PercentRange == percentRange)
                    return point;
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return checkPoints.GetEnumerator();
        }

        public void Add(CheckPoint checkPoint)
        {
            checkPoint.CalcError(RangeMin, RangeMax);
            checkPoint.CheckResult(ClassPrecision, MarginCoefficient);
            checkPoints.Add(checkPoint);
        }

        public void Clear()
        {
            checkPoints.Clear();
        }

        public bool? GetResume()
        {
            if (checkPoints.Count == 0)
                return null;
            bool? res = true;
            foreach (var point in checkPoints)
            {
                res &= point.CheckResult(ClassPrecision, MarginCoefficient);
            }
            return res;
        }
    }
}
