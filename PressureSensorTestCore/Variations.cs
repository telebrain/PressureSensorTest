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

        public void Add(VariationPoint point)
        {
            variationPoints.Add(point);
        }

        public IEnumerator GetEnumerator()
        {
            return variationPoints.GetEnumerator();
        }

        public VariationPoint this[int index]
        {
            get { return variationPoints[index]; }
        }

        public bool? GetResume()
        {
            if (variationPoints.Count == 0)
                return null;
            foreach (var point in variationPoints)
            {
                if (!point.Resume)
                    return false;

            }
            return true;
        }

        public int Count { get { return variationPoints.Count; } }
    }
}
