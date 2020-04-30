using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressureRack;

namespace PressSystems
{
    public class PressRackInfo: PressSystemInfo
    {
        public PressRackInfo(PressureRack.PressSystemInfo info)
        {
            base.PressureHi = info.PressureHi;
            base.PressureLo = info.PressureLo;

            Controllers = new PressControllerInfo[info.Controllers.Length];          
            for (int i = 0; i < info.Controllers.Length; i ++)
            {
                Controllers[i].SN = info.Controllers[i].SN;
                Controllers[i].Precision = info.Controllers[i].PrecClass;
                Controllers[i].PressureLo = info.Controllers[i].RangeLo;
                Controllers[i].PressureHi = info.Controllers[i].RangeHi;
            }          
        }
    }
}
