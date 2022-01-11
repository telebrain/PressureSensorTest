using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public interface IRelayDevice
    {
        void Init();

        void WriteSettings(double sp, double hysteresis);
    }
}
