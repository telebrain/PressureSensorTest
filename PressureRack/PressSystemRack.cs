using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PressureRack
{
    public class PressSystemRack
    {
        public PressSystemInfo PressSystemInfo { get; private set; } = null;
        public string Ip { get; set; }
        public int Port { get; set; }
        public int ChannelOut { get; set; }
        public bool IsConnect { get; private set; }
        public int PsysChannel { get; private set; }
        public float SP { get; private set; }
        public Exception CurrentException { get; private set; }

        float pv;
        public float PV { get { return pv; } }

        float bar;
        public float Bar { get { return bar; } }

        bool inlim;
        public bool Inlim { get { return inlim; } }

        public event EventHandler UpdPsysVarEvent;
        public event EventHandler PsysConnectEvent;
        public event EventHandler ExceptionEvent;


        CancellationTokenSource cts;
        CancellationToken token;
        PsysExch exchange;
        readonly object syncRoot = new object();
        Task psysTask;


        public PressSystemRack(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public void Stop()
        {
            if (cts != null && psysTask != null)
            {
                cts.Cancel();
                psysTask.Wait();
            }
            IsConnect = false;
        }

        public void ReadPressSystemInfo()
        {
            try
            {
                exchange = new PsysExch(Ip, Port);

                PressSystemInfo = new PressSystemInfo();
                PressSystemInfo.Controllers = exchange.GetDruckInfo();

                exchange.GetPressureLimits(out float PressureLo, out float PressureHi);

                PressSystemInfo.PressureLo = PressureLo;
                PressSystemInfo.PressureHi = PressureHi;

                exchange.Close();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
                ExceptionEvent?.Invoke(this, new EventArgs());
            }
        }

        public void StartPsys(int channel)
        {
            ChannelOut = channel;
            psysTask = WorkPsysAsync();
        }

        public async Task WorkPsysAsync()
        {
            try
            {
                CurrentException = null;
                cts = new CancellationTokenSource();
                token = cts.Token;
                exchange = new PsysExch(Ip, Port);
                await Task.Run(() => PsysAsyncOperation());
            }
            catch (Exception ex)
            {
                CurrentException = ex;
                ExceptionEvent?.Invoke(this, new EventArgs());
            }
        }

        public void WriteSP(float sp, int channel)
        {
            try
            {
                lock (syncRoot)
                {
                    exchange.SendSP(sp, channel);
                }
                WaitResultSensSP();
                PsysChannel = channel;
                SP = sp;
            }
            catch (Exception ex)
            {
                CurrentException = ex;
                ExceptionEvent?.Invoke(this, new EventArgs());
            }
        }

        private void WaitResultSensSP()
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

        private void PsysAsyncOperation()
        {
            WaitConnect();
            CycleReadVariablesPsys();
            exchange.Close();
        }

        private void WaitConnect()
        {
            exchange.TryConnectPsys(ChannelOut);
            while (!token.IsCancellationRequested)
            {
                if (exchange.WaitConnectResult())
                {
                    IsConnect = true;
                    PsysConnectEvent?.Invoke(this, new EventArgs());
                    return;
                }
                Thread.Sleep(500);
            }
        }

        private void CycleReadVariablesPsys()
        {
            while (!token.IsCancellationRequested)
            {
                lock (syncRoot)
                {
                    exchange.ReadVar(out pv, out bar, out inlim);
                }
                UpdPsysVarEvent?.Invoke(this, new EventArgs());
                Thread.Sleep(500);
            }
        }
    }
}
