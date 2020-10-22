using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressSystems;

namespace JE_PACE
{
    public class Commands : IPressSystemCommands
    {
        Pace pace = null;

        public Commands(Pace pace)
        {
            this.pace = pace;
        }

        public void Connect(int outChannelNumber, CancellationToken cancellationToken)
        {
            pace.Connect();
        }

        public void DisableControl()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public PressSystemInfo ReadInfo()
        {
            throw new NotImplementedException();
        }

        public PressSystemVariables ReadSysVar()
        {
            throw new NotImplementedException();
        }

        public void WriteSP(int controller, double SP, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
