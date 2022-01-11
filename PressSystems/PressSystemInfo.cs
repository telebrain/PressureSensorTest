using System;


namespace PressSystems
{
    public class PressSystemInfo
    {
        // Все величины в Па

        // Информация о контроллерах давления в системе
        public PressControllersList Controllers { get; set; }

        // Фактические границы диапазона системы, обусловленные давлением источников, питающих контроллеры
        // и прочностью арматуры. Могут быть меньше диапазона контроллеров, входящих в состав системы
        public double RangeLo { get; set; } 
        public double RangeHi { get; set; }
        // public bool BarometrAvaiable { get; set; }

        // Давление, равное полному вакууму, относительно атмосферного
        // public const int VacuumPressure = -100000;

        // Допустимое отношение диапазона, в котором можно задать давление к требуемому диапазону
        readonly double ExtrapolationDownCoef = 0.8; // При экстаполяции вниз
        readonly double ExtrapolationUpCoef = 1;  // При экстраполяции вверх

        public bool CheckRange(double targetMax, double targetMin)
        {
            if (!CheckRangeMax(targetMax, targetMin))
                return false;
            return CheckRangeMin(targetMax, targetMin);
        }

        // Проверка возможности задания давления исходя из значения положительного источника давления
        private bool CheckRangeMax(double targetMax, double targetMin)
        {
            bool res = true;
            if (targetMax > RangeHi)
            {
                res = Math.Abs((RangeHi - targetMin) / (targetMax - targetMin)) >= ExtrapolationUpCoef;
            }

            return res;
        }

        // Проверка возможности задания давления исходя из значения отрицательного источника давления
        private bool CheckRangeMin(double targetMax, double targetMin)
        {
            bool res = true;          
            if (targetMin < RangeLo)
            {
                if (targetMax > RangeLo)
                {
                    res = Math.Abs((targetMax - RangeLo) / (targetMax - targetMin)) >= ExtrapolationDownCoef;
                }
                else
                {
                    res = false;
                }
            } 
            return res;
        }

        // Поиск номера контроллера. Если контроллер не найден, возвращает -1
        public int SearshController(double sp, double targetRangeMax, double targetRangeMin)
        {
            int targetMax = (int)targetRangeMax;
            int targetMin = (int)targetRangeMin;
            if (targetRangeMax < targetRangeMin)
            {
                targetMax = (int)targetRangeMin;
                targetMin = (int)targetRangeMax;
            }
            foreach (PressControllerInfo controller in Controllers)
            {
                if (controller.IsEnabled)
                {
                    // Проверка диапазона контроллера
                    bool checkRange = sp >= 0 ? targetMax <= (int)controller.RangeHi : 
                        targetMin >= (int)controller.RangeLo;
                    if (checkRange)
                        return controller.Number;
                }
            }

            return -1; // В пневмосистеме не найден ни один контроллер, подходящий по диапазону

        }  
    }
}
