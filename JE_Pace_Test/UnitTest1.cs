using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JE_PACE;
using System.Diagnostics;
using System.Threading;

namespace JE_Pace_Test
{
    [TestClass]
    public class UnitTest1
    {
        string portName = "COM5";

        [TestMethod]
        public void ReadSN()
        {
            var exch = new ComExchange(portName);
            using (var pace = new Pace(exch))
            {
                pace.Connect();
                string sn = pace.GetSn();
                string snModul1 = pace.GetModulSn();
                string snModul2 = pace.GetModulSn(1);
                Debug.WriteLine($"Заводской номер PACE {sn}");
                Debug.WriteLine($"Заводской номер модуля 1 {snModul1}");
                Debug.WriteLine($"Заводской номер модуля 2 {snModul2}");
            }
        }
        [TestMethod]
        public void CheckBarometr()
        {
            var exch = new ComExchange(portName);
            using (var pace = new Pace(exch))
            {
                pace.Connect();
                string message = pace.CheckBarometr() ? "Барометр в модуле 1 установлен" :
                    "Барометр в модуле 1 не установлен";
                string message2 = pace.CheckBarometr() ? "Барометр в модуле 2 установлен" :
                    "Барометр в модуле 2 не установлен";
                Debug.WriteLine(message);
                Debug.WriteLine(message2);
            }
        }
        [TestMethod]
        public void GetRange()
        {
            var exch = new ComExchange(portName);
            using (var pace = new Pace(exch))
            {
                pace.Connect();
                int range = (int)pace.GetRange(out double lo, out double hi);
                int range2 = (int)pace.GetRange(out double lo2, out double hi2, 1);

                Debug.WriteLine($"Диапазон 1-го модуля {range} Па");
                Debug.WriteLine($"Лимит источника + 1-го модуля {hi} Па");
                Debug.WriteLine($"Лимит источника - 1-го модуля {lo} Па");

                Debug.WriteLine($"Диапазон 2-го модуля {range2} Па");
                Debug.WriteLine($"Лимит источника + 2-го модуля {hi2} Па");
                Debug.WriteLine($"Лимит источника - 2-го модуля {lo2} Па");
            }
        }
        [TestMethod]
        public void WriteUnit()
        {
            var exch = new ComExchange(portName);
            using (var pace = new Pace(exch))
            {
                pace.Connect();
                pace.WriteUnitWithCheck(PaceUnitsEnum.KPA);
                pace.WriteUnitWithCheck(PaceUnitsEnum.KPA, 1);


                Debug.WriteLine($"Единицы измерения 1-го модуля {pace.Units[0]}");
                Debug.WriteLine($"Единицы измерения 2-го модуля {pace.Units[1]}");

            }
        }

        [TestMethod]
        public void WriteSP()
        {
            var exch = new ComExchange(portName);
            using (var pace = new Pace(exch))
            {
                pace.Connect();
                pace.WriteSpWithCheck(20000);
                pace.WriteSpWithCheck(30000, 1);
                Thread.Sleep(3000);
                pace.WriteSpWithCheck(0);
                pace.WriteSpWithCheck(0, 1);

            }
        }
        [TestMethod]
        public void ReadPressures()
        {
            var exch = new ComExchange(portName);
            using (var pace = new Pace(exch))
            {
                pace.Connect();
                double press = pace.ReadPressure();
                double srcMinus = pace.ReadSourceMinus();
                double srcPlus = pace.ReadSourcePlus();

                double press2 = pace.ReadPressure(1);
                double srcMinus2 = pace.ReadSourceMinus(1);
                double srcPlus2 = pace.ReadSourcePlus(1);

                Debug.WriteLine($"Давление 1-го модуля {press}, Па");
                Debug.WriteLine($"Давление источника + 1-го модуля {srcPlus}, Па");
                Debug.WriteLine($"Давление источника - 1-го модуля {srcMinus}, Па");

                Debug.WriteLine($"Давление 2-го модуля {press2}, Па");
                Debug.WriteLine($"Давление источника + 2-го модуля {srcPlus2}, Па");
                Debug.WriteLine($"Давление источника - 2-го модуля {srcMinus2}, Па");
            }
        }
    }
}
