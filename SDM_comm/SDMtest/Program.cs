using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDM_comm;
using System.Globalization;

namespace SDMtest
{
    class Program
    {
        static void Main(string[] args)
        {
            var sdm = new Ammetr("10.4.14.103");

            sdm. += Sdm_UpdMeasureResult;

            try
            {

                sdm.Start();
                Console.ReadKey();

                sdm.Stop();
                Console.ReadKey();
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
                // Console.WriteLine(double.Parse("-5.57587244E-0", new CultureInfo("en-ru", false).NumberFormat));




            }

        private static void Sdm_UpdMeasureResult(object sender, double e)
        {
            Console.WriteLine(e);
        }

        private static void ErrorHanding(string error)
        {
            Console.WriteLine(error);
        }
    }
}
