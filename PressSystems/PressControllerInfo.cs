using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressSystems
{
    // Информация о контроллере давления
    public class PressControllerInfo
    {
        public int Number { get; set; } // Номер контроллера в пневмосистеме
        public bool IsEnabled { get; set; } // Флаг активности (включен - выключен)
        public string SN { get; set; } = ""; // Серийный номер
        public double RangeLo { get; set; } // Нижняя граница диапазона
        public double RangeHi { get; set; } // Верхняя граница диапазона
        public double Precision { get; set; }  // Класс точности  
    }
}
