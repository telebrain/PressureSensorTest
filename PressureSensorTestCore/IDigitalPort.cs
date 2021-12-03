using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public interface IDigitalPort: IDisposable
    {
        void Init();
        double GetPressurePa();
    }
}
