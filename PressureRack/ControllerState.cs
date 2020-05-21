using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureRack
{
    public struct ControllerState // Состояние одного контроллера пневмосистемы 
    //Принимается от пневмосистемы по запросу "INFO <X>;" 
    {
        public bool IsEnabled { get; private set; }
        public string SN { get; private set; }
        public double RangeLo { get; private set; }
        public double RangeHi { get; private set; }
        public double PrecClass { get; private set; }
        

        public ControllerState(string stateStr) // Декодирует принятую из сокета строку в информацию
        {
            IsEnabled = (Parsing.ExtractStringParametr("STATE:", stateStr) == "ON");
            RangeLo = Convert.ToDouble(Parsing.ExtractStringParametr("LO:", stateStr));
            RangeHi = Convert.ToDouble(Parsing.ExtractStringParametr("HI:", stateStr));
            PrecClass = Convert.ToDouble(Parsing.ExtractStringParametr("PREC:", stateStr));
            SN = Parsing.ExtractStringParametr("SN:", stateStr);
        }
    }
}
