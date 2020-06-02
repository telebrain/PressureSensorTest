using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class Variations : IEnumerable
    {
        List<VariationPoint> variationPoints = new List<VariationPoint>();

        public int Count { get { return variationPoints.Count; } }

        public VariationPoint this[int index]
        {
            get { return variationPoints[index]; }
        }

        public void Add(VariationPoint point)
        {
            variationPoints.Add(point);
        }

        public IEnumerator GetEnumerator()
        {
            return variationPoints.GetEnumerator();
        }

        public bool? GetResume()
        {
            if (variationPoints.Count == 0)
                return null;
            bool res = true;
            foreach (var point in variationPoints)
            {
                res &= point.Resume;
            }
            return res;
        }

        
    }
}
