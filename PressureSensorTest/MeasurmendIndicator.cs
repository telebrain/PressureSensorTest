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
        public double Pressure { get; private set; }
        public bool Inlim { get; private set; }
        public double Current { get; private set; }

        readonly IAmmetr ammetr;
        readonly IPressSystem psys;

        public event EventHandler UpdDataEvent;

        readonly bool absolutePressure;

        public MeasurmendIndicator(IAmmetr ammetr, IPressSystem psys, bool absolutePressure)
        {
            this.ammetr = ammetr;
            this.psys = psys;
            this.absolutePressure = absolutePressure;
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

        private async Task CycleRead(CancellationToken cancellationToken)
        {
            while(true)
            {
                if(cancellationToken.IsCancellationRequested)
                    break;
                UpdValue();
                await Task.Delay(100);
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
                Pressure = absolutePressure ? psys.PressSystemVariables.Pressure + psys.PressSystemVariables.Barometr : 
                    psys.PressSystemVariables.Pressure;
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
