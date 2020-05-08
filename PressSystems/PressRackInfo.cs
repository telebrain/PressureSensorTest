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
            RangeHi = info.PressureHi;
            RangeLo = info.PressureLo;

            Controllers = new PressControllersList();          
            for (int i = 0; i < info.Controllers.Length; i ++)
            {
                var controller = new PressControllerInfo()
                {
                    Number = i + 1,
                    IsEnabled = info.Controllers[i].State,
                    SN = info.Controllers[i].SN,
                    Precision = info.Controllers[i].PrecClass,
                    RangeLo = info.Controllers[i].RangeLo,
                    RangeHi = info.Controllers[i].RangeHi
                };
                Controllers.Add(controller);
               
            }          
        }
    }
}
