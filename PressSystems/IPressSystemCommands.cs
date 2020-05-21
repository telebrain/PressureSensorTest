using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PressSystems
{
    public interface IPressSystemCommands
    {
        void Connect(int outChannelNumber, CancellationToken cancellationToken);

        PressSystemVariables ReadSysVar();

        PressSystemInfo ReadInfo();

        void WriteSP(int controller, double SP, CancellationToken cancellationToken);

        void DisableControl();

        void Disconnect();
    }
}
