using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SDM_comm;

namespace SDM_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var ammetr = new Ammetr("10.4.14.105", CurrentTypeEnum.DC, CurrentUnitsEnum.mA, 20);
            ammetr.UpdMeasureResult += Meas;
            ammetr.StartCycleMeasureCurrent();
           
            Console.ReadKey();
            

            ammetr.Stop();
        }

        static void Meas(object sender, EventArgs e)
        {
            Console.WriteLine(((Ammetr)sender).Current);
        }
    }
}
