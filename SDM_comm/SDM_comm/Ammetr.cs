using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SDM_comm
{
    public class Ammetr: IAmmetr 
    {
        const int Port = 5024;
        readonly string ip = "";
        Transport transport = null;
        SDM_Commands commands = null;
        double multipler = 1;

        public event EventHandler UpdMeasureResult;
        public event EventHandler ExceptionEvent;
        public event EventHandler ConnectEvent;
        public event EventHandler DisconnectEvent;

        public double Current { get; private set; }

        public long Timestamp { get; private set; }

        public Exception Exception { get; private set; }

        public bool StateConnect { get; private set; }

        public CurrentTypeEnum CurrentType { get; private set; }

        public CurrentUnitsEnum Units { get; private set; }

        public int Range { get; private set; }

        public Ammetr(string ip, CurrentTypeEnum currentType, CurrentUnitsEnum units, int range)
        {
            this.ip = ip;
            CurrentType = currentType;
            Units = units;
            multipler = Units == CurrentUnitsEnum.A ? 1 : 1000;
                
            Range = range;
        }

        CancellationTokenSource cts;
        Task cycleTask;

        public void StartCycleMeasureCurrent()
        {
            cts = new CancellationTokenSource();
            if (!StateConnect)
            {
                Exception = null;
                cycleTask = StartCycle(cts.Token);
                
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
                await Task.Run(() => CycleMeasureCurrent(cancellationToken));
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
            finally
            {
                transport.Dispose();
                DisconnectEvent?.Invoke(this, new EventArgs());
            }

        }

        private void CycleMeasureCurrent(CancellationToken cancellationToken)
        {
            transport = new Transport(ip, Port);
            transport.Connect();
            commands = new SDM_Commands(transport);
            commands.SetCurrentRange(CurrentType, Range, Units);
            commands.SetSamples(1);
            commands.InitCommand();
            StateConnect = true;
            ConnectEvent?.Invoke(this, new EventArgs());
            while (!cancellationToken.IsCancellationRequested)
            {
                Current = commands.ReadMeasValue(5) * multipler;
                Timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                UpdMeasureResult?.Invoke(this, new EventArgs());
                Thread.Sleep(300);
            }
            
        }
    }
}
