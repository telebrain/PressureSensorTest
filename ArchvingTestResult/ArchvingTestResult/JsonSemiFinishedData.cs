using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchvingTestResult
{
    public class JsonSemiFinishedData
    {
        public JsonSemiFinishedData() { }

        public JsonSemiFinishedData(long sn)
        {
            deviceSN = sn;
        }

        public long deviceSN { get; set; }
    }
}
