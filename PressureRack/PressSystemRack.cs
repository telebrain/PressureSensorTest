using System;
using System.Threading.Tasks;
using System.Threading;

namespace PressureRack
{
    public class PressSystemRack
    {
        public PressSystemInfo PressSystemInfo { get; private set; } = null;
        public string Ip { get; set; }
        public int Port { get; set; }

        PsysExch exchange;
        readonly object syncRoot = new object();


        public PressSystemRack(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public void ReadPressSystemInfo()
        {
            exchange = new PsysExch(Ip, Port);

            PressSystemInfo = new PressSystemInfo();
            PressSystemInfo.Controllers = exchange.GetDruckInfo();

            exchange.GetPressureLimits(out double PressureLo, out double PressureHi);

            PressSystemInfo.PressureLo = PressureLo;
            PressSystemInfo.PressureHi = PressureHi;

            exchange.Close();
        }

        public void WriteSP(double sp, int controller, CancellationToken token)
        {
            lock (syncRoot)
            {
                exchange.SendSP(sp, controller);
            }
            WaitResultSensSP(token);
        }

        private void WaitResultSensSP(CancellationToken token)
        {
            bool result = false;
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                lock (syncRoot)
                {
                    result = exchange.GetSysState();
                }
                if (result) break;
                Thread.Sleep(500);
            }
        }

        

        public void Connect(int channelOut, CancellationToken token)
        {
            exchange.TryConnectPsys(channelOut);
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (exchange.WaitConnectResult())
                    return;
                
                Thread.Sleep(500);
            }
        }

        public void ReadVariablesPsys(out double pressure, out double bar, out bool inLim, out long timestamp)
        {
            lock (syncRoot)
            {
                exchange.ReadVar(out pressure, out bar, out inLim);
                // С таймстемп пока заглушка
                timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
        }
    }
}
