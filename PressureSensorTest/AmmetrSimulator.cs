using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDM_comm;
using PressSystems;

namespace PressureSensorTest
{
    public class AmmetrSimulator : IAmmetr
    {
        public double Current { get; private set; }

        public Exception Exception { get; private set; }

        public bool StateConnect { get; private set; }

        public long Timestamp { get; private set; }

        public CurrentTypeEnum CurrentType => CurrentTypeEnum.DC;

        public CurrentUnitsEnum Units => CurrentUnitsEnum.mA;

        public int Range => 200;

        public event EventHandler UpdMeasureResult;

        public event EventHandler ExceptionEvent;
        public event EventHandler ConnectEvent;
        public event EventHandler DisconnectEvent;

        IPressSystem pressSystem;
        double rangeMin;
        double rangeMax;
        double precision;
        Random random;
        bool absoluteType = false;

        public AmmetrSimulator(IPressSystem pressSystem, double rangeMin, double rangeMax, double precision, bool absoluteType)
        {
            this.pressSystem = pressSystem;
            this.rangeMin = rangeMin;
            this.rangeMax = rangeMax;
            this.precision = precision;
            this.absoluteType = absoluteType;
            random = new Random();
        }

        CancellationTokenSource cts;
        CancellationToken cancellationToken;
        Task cycleTask;

        public void StartCycleMeasureCurrent()
        {
            //updPressure = new AutoResetEvent(false);
            //pressSystem.UpdateMeasures += (obj, e) => updPressure.Set();
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
            cycleTask.GetAwaiter().GetResult();
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

        int i = 0;
        private void CycleMeasureCurrent()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Симуляция аварии
                //i++;
                //if (i >= 10)
                //    throw new SDM_ErrException("Прервана связь с прибором АКИП 2101");
                Thread.Sleep(1000);
                //cancellationToken.ThrowIfCancellationRequested();
                double shift = absoluteType ? pressSystem.PressSystemVariables.Barometr*(-1) : rangeMin;
                double span = rangeMax - rangeMin;
                double point = pressSystem.ConnectState ? (pressSystem.PressSystemVariables.Pressure - shift) / span : shift/span;
                double error = (2 * random.NextDouble() * precision - precision) / 100;
                Current = (point + error) * 16 + 4;
                Timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                UpdMeasureResult?.Invoke(this, new EventArgs());
                
            }
        }

        
    }
}
