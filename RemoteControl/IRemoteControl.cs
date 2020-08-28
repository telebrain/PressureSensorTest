using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl
{
    public interface IRemoteControl: IDisposable
    {
        event EventHandler StartProcessEvent;
        event EventHandler CancellationEvent;

        void ChangeState(StateProcessEnum stateProcess);

        void StartListening();

    }
}
