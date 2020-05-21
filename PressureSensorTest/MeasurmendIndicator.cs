using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressSystems;
using SDM_comm;

namespace PressureSensorTest
{
    public class MeasurmendIndicator
    {
        public double Pressure;
        public bool Inlim;
        public double Current;

        readonly IAmmetr ammetr;
        readonly IPressSystem psys;

        public event EventHandler UpdDataEvent;

        public MeasurmendIndicator(IAmmetr ammetr, IPressSystem psys)
        {
            this.ammetr = ammetr;
            this.psys = psys;
            
        }

        CancellationTokenSource cts;

        public async void Start()
        {
            cts = new CancellationTokenSource();
            await Task.Run(() => CycleRead(cts.Token));
        }

        public void Stop()
        {
            cts.Cancel();
        }

        private void CycleRead(CancellationToken cancellationToken)
        {
            while(true)
            {
                if(cancellationToken.IsCancellationRequested)
                    break;
                UpdValue();
                Thread.Sleep(100);
            }
        }

        private void UpdValue()
        {
            if (ammetr != null && ammetr.StateConnect)
                Current = ammetr.Current;
            else
                Current = 0;

            if (psys != null && psys.ConnectState)
            {
                Pressure = psys.PressSystemVariables.Pressure;
                Inlim = psys.PressSystemVariables.InLim;
            }
            else
            {
                Pressure = 0;
                Inlim = true;
            }
            UpdDataEvent?.Invoke(this, new EventArgs());
        }
    }
}
