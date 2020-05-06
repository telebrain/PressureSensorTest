using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureRack
{
    public class DruckState // Состояние одного контроллера пневмосистемы 
    //Принимается от пневмосистемы по запросу "INFO <X>;" 
    {
        public bool State { get; private set; }
        public string SN { get; private set; }
        public double RangeLo { get; private set; }
        public double RangeHi { get; private set; }
        public double PrecClass { get; private set; }
        

        public DruckState(string str) // Декодирует принятую из сокета строку в информацию
        {
            State = (Decoder.ExtractData("STATE:", str) == "ON");
            RangeLo = Convert.ToDouble(Decoder.ExtractData("LO:", str));
            RangeHi = Convert.ToDouble(Decoder.ExtractData("HI:", str));
            PrecClass = Convert.ToDouble(Decoder.ExtractData("PREC:", str));
            SN = Decoder.ExtractData("SN:", str);
        }
    }
}
