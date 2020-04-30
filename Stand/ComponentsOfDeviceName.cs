using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwenPressureDevices
{
    public struct ComponentsOfDeviceName
    {
        public string Title { get; }
        public string RangeType { get; }
        public string PressureRange { get; }
        public string Modification { get;  }
        public string Precision { get; }
        public string ThreadType { get;  }

        public ComponentsOfDeviceName(string title, string rangeType, string pressureRange, string modifications, 
            string threadType, string precision)
        {
            Title = title;
            RangeType = rangeType;
            PressureRange = pressureRange;
            Modification = modifications;
            ThreadType = threadType;
            Precision = precision;
        }


    }
}
