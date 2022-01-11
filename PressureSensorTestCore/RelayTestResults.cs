using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class RelayTestResults: IEnumerable
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
}
