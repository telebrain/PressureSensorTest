using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchvingTestResult
{
    [Serializable]
    public class JsonTestPointData
    {        
        public int channelNumber { get; set; }
        public int sensorType { get; set; }
        public int checkPoint { get; set; }
        public JsonParametr[] dataMetrological { get; set; }
    }
}
