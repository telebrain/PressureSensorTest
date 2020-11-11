using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchvingTestResult
{
    [Serializable]
    public struct JsonParametr
    {
        public int paramID { get; set; }
        public double value { get; set; }

        public JsonParametr(int paramID, float value)
        {
            this.paramID = paramID;
            this.value = value;
        }

        public JsonParametr(int paramID, double value)
        {
            this.paramID = paramID;
            this.value = value;
        }
    }
}
