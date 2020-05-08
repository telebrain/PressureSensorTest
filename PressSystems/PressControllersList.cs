using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressSystems
{
    public class PressControllersList: IEnumerable
    {
        List<PressControllerInfo> controllers = new List<PressControllerInfo>();


        // Все контроллеры, добавленные в список, будут отсортированы по верхнему значению диапазона
        public void Add(PressControllerInfo pressControllerInfo)
        {
            controllers.Add(pressControllerInfo);
            controllers.Sort(new ComparerControllers<PressControllerInfo>());
        }

        public IEnumerator GetEnumerator()
        {
            return controllers.GetEnumerator();
        }

        public int Count
        {
            get { return controllers.Count; }
        }

        public PressControllerInfo this[int index]
        {
            get
            {
                return controllers[index];
            }
        }

        class ComparerControllers<T> : IComparer<T>
            where T : PressControllerInfo
        {
            public int Compare(T x, T y)
            {
                if (x.RangeHi > y.RangeHi)
                    return 1;
                if (x.RangeHi < y.RangeHi)
                    return -1;
                return 0;
            }
        }
    }
}
