using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PressureSensorTest
{
    public class WaitContinue
    {
        bool waitOne;

        public event EventHandler ContinueRequest;

        public event EventHandler SelectionRequest;


        public void Continue()
        {
            waitOne = false;
        }

        

        public void Wait(CancellationToken cancellation)
        {
            waitOne = true;
            ContinueRequest?.Invoke(this, new EventArgs());
            while (waitOne)
            { 
                cancellation.ThrowIfCancellationRequested();
                Thread.Sleep(100);
            }
        }

        public void WaitSelection(CancellationToken cancellation)
        {
            waitOne = true;
            SelectionRequest?.Invoke(this, new EventArgs());
            while (waitOne)
            {
                cancellation.ThrowIfCancellationRequested();
                Thread.Sleep(100);
            }
        }
    }
}
