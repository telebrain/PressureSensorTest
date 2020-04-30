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
        public float value { get; set; }

        public JsonParametr(int paramID, float value)
        {
            this.paramID = paramID;
            this.value = value;
        }
    }
}
