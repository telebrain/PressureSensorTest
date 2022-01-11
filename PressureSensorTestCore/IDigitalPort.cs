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

    [Serializable]
    public class OpenConnectPortException : Exception { }
    [Serializable]
    public class ConnectException : Exception { }
    [Serializable]
    public class LostConnectException : Exception { }
}
