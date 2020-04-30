using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureRack
{
    public class PressSystemInfo
    {
        // Информация о контроллерах давления в системе
        public DruckState[] Controllers { get; set; }

        // Фактические границы диапазона системы, обусловленные давлением источников, питающих контроллеры
        // и прочностью арматуры. Могут быть меньше диапазона контроллеров, входящих в состав системы
        public float PressureLo { get; set; }
        public float PressureHi { get; set; }
    }
}
