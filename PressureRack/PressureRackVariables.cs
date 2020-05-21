using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureRack
{
    public struct PressureRackVariables
    {
        // Все давления в кПа

        // Давление
        public double Pressure { get; set; }
        // Флаг выхода текущего контроллера на уставку
        public double Barometr { get; set; }
        // Флаг выхода текущего контроллера на уставку
        public bool InLim { get; set; }
        // Метка времени обновления переменных
        public long TimeStamp { get; set; }
    }
}
