using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDM_comm;
using PressSystems;

namespace PressureSensorTestCore
{
    public class AmmetrSimulator : IAmmetr
    {
        public double Current { get; private set; }

        public Exception Exception { get; private set; }

        public bool StateConnect { get; private set; }

        public event EventHandler<double> UpdMeasureResult;

        public event EventHandler ExceptionEvent;
        public event EventHandler ConnectEvent;
        public event EventHandler DisconnectEvent;

        IPressSystem pressSystem;
        double rangeMin;
        double rangeMax;
        double precision;
        Random random;

        public AmmetrSimulator(IPressSystem pressSystem, double rangeMin, double rangeMax, double precision)
        {
            this.pressSystem = pressSystem;
            this.rangeMin = rangeMin;
            this.rangeMax = rangeMax;
            this.precision = precision;
            random = new Random();
        }

        CancellationTokenSource cts;
        CancellationToken cancellationToken;
        Task cycleTask;

        public void StartCycleMeasureCurrent()
        {
            if (!StateConnect)
            {
                Exception = null;
                cts = new CancellationTokenSource();
                cancellationToken = cts.Token;
                cycleTask = StartCycle(cancellationToken);
                StateConnect = true;
            }
            ConnectEvent?.Invoke(this, new EventArgs());
        }

        public void Stop()
        {
            cts.Cancel();
            StateConnect = false;
            DisconnectEvent?.Invoke(this, new EventArgs());
        }

        private async Task StartCycle(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() => CycleMeasureCurrent());
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (Exception == null)
                {
                    Exception = e;
                    ExceptionEvent?.Invoke(this, new EventArgs());
                }
            }

        }

        //  int i = 0;
        private void CycleMeasureCurrent()
        {
            while (true)
            {
                // Симуляция аварии
                //i++;
                //if (i >= 10)
                //    throw new Exception("Нет связи с мультиметром");

                cancellationToken.ThrowIfCancellationRequested();
                double point = (pressSystem.Pressure + rangeMin) / (rangeMax - rangeMin);
                double error = (2 * random.NextDouble() * precision - precision) / 100;
                Current = (point + error) * 16 + 4;
                UpdMeasureResult?.Invoke(this, Current);
                Thread.Sleep(300);
            }
        }
    }
}
