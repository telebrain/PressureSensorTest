using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTest
{
    [Serializable]
    public class JsonReportSettings
    {
        public int StandId { get; set; }
        public string LineId { get; set; }
        public string StandHardwareVer { get; set; }
        public bool ArchivingJsonFile { get; set; }
        public bool UsedFtp { get; set; }
        public string ArchivingPath { get; set; } = "";
        public int MaxCommunicationBreakWithArchive { get; set; }
        public int[] PointsCode { get; set; } = new int[] { 3, 6, 8, 10, 13 };

    }
}
