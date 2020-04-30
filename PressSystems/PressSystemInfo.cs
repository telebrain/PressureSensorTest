using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressSystems
{
    public class PressSystemInfo
    {
        // Все величины в Па

        // Информация о контроллерах давления в системе
        public PressControllerInfo[] Controllers { get; set; }

        // Фактические границы диапазона системы, обусловленные давлением источников, питающих контроллеры
        // и прочностью арматуры. Могут быть меньше диапазона контроллеров, входящих в состав системы
        public double PressureLo { get; set; } 
        public double PressureHi { get; set; }
        public bool BarometrAvaiable { get; set; }

        const double precisionCoef = 3;

        // Давление, равное полному вакууму, относительно атмосферного
        const int VacuumPressure = -100000;

        // Допустимое отношение диапазона, в котором можно задать давление к требуемому диапазону
        readonly double ExtrapolationDownCoef = 0.8F; // При экстаполяции вниз
        readonly double ExtrapolationUpCoef = 1;  // При экстраполяции вверх

        // Проверка возможности задания давления исходя из значения положительного источника давления
        public bool CheckRangeMax(double targetMax, double targetMin)
        {
            bool res = true;
            if (targetMax > PressureHi)
            {
                res = Math.Abs((PressureHi - targetMin) / (targetMax - targetMin)) >= ExtrapolationUpCoef;
            }

            return res;
        }

        // Проверка возможности задания давления исходя из значения отрицательного источника давления
        public bool CheckRangeMin(double targetMax, double targetMin)
        {
            bool res = targetMin >= VacuumPressure;
            
            if (targetMin < PressureLo)
            {
                if (targetMax > PressureLo)
                {
                    double c = Math.Abs((targetMax - PressureLo) / (targetMax - targetMin));
                    res = Math.Abs((targetMax - PressureLo) / (targetMax - targetMin)) >= ExtrapolationDownCoef;
                }
                else
                {
                    res = false;
                }
            } 
            return res;
        }

        // Проверка возможности задания давления исходя из значения отрицательного источника давления
        // для типа ДВ
        public bool CheckRangeMin(double targetDV)
        {
            return PressureLo / targetDV >= ExtrapolationDownCoef && targetDV >= VacuumPressure;
        }

        // Поиск номера контроллера (1...4). Если контроллер не найден, возвращает -1
        public int SearshController(double targetPressure, double span, double precisionClass)
        {
            int res = -1;
            for (int i = Controllers.Length - 1; i >= 0; i--)
            {
                if (Controllers[i].Enabled)
                {
                    double controllerRangeMin = Controllers[i].PressureHi * Controllers[i].Precision * precisionCoef / precisionClass;
                    if (Math.Abs(span) >= controllerRangeMin)
                    {
                        if (targetPressure > 0)
                        {
                            if (Controllers[i].PressureHi >= targetPressure)
                            {
                                res = i + 1;
                                break;
                            }
                        }
                        else
                        {
                            if (Controllers[i].PressureLo <= targetPressure)
                            {
                                res = i + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return res;
           
        }
  
    }
}
