using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl
{
    interface IRemoteControl
    {
        event EventHandler StartProcessEvent;

        event EventHandler CancellationEvent;

        void SendResult(bool state);

        void SendError(Exception exception);
    }
}
