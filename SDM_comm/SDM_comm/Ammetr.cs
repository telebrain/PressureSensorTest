using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SDM_comm
{
    public class Ammetr 
    {
        const int Port = 5024;
        readonly string ip = "";
        Transport transport = null;
        SDM_Commands commands = null;

        public event EventHandler<double> UpdMeasureResult;
        public double Current { get; private set; }
        public Exception Exception { get; private set; }

        public Ammetr (string ip)
        {
            this.ip = ip;
        }

        public void StartCycleMeasureCurrent(CancellationToken cancellationToken)
        {
            try
            {
                using (transport = new Transport(ip, Port))
                {
                    transport.Connect();
                    commands = new SDM_Commands(transport);
                    commands.SetCurrentRange(CurrentType.DC, 200, CurrentUnitsEnum.mA);
                    commands.InitCommand();
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        Current = commands.ReadMeasValue(5);
                        UpdMeasureResult?.Invoke(this, Current);
                        Thread.Sleep(1000);
                    }
                }
            }
            catch(Exception ex)
            {
                Exception = ex;
            }
        }
    }
}
