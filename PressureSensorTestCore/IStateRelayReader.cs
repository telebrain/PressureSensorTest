using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public interface IStateRelayReader
    {
        bool[] StateRelay { get; }

        event EventHandler StateReadEvent; 
    }
}
