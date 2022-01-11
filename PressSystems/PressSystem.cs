using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PressSystems
{
    public sealed class PressSystem : IPressSystem, IDisposable
    {
        const double RateIsMax = -1;

        public PressSystemInfo Info { get; private set; }

        public double CurrentSP { get; private set; }


        public int CurrentController { get; private set; }

        public int CurrentOutputChannel { get; private set; } = 1;

        public PressSystemVariables PressSystemVariables { get; private set; }

        public bool ConnectState { get; private set; }

        public int MaxTimeSetPressureOperation { get; set; } = 30; // В секундах

        const int DelayCheckInlim = 5; // в секундах

        Exception exception = null;
        public Exception Exception
        {
            get { return exception; }
            private set
            {
                if (exception == null)
                {
                    exception = value;
                    ExceptionEvent?.Invoke(this, new EventArgs());
                }
            }
        }

        private void ClearException()
        {
            exception = null;
        }

        public event EventHandler UpdateMeasures;
        public event EventHandler ExceptionEvent;
        public event EventHandler BeginConnectEvent;
        public event EventHandler ConnectEvent;
        public event EventHandler DisconnectEvent;

        readonly IPressSystemCommands commands;

        public PressSystem(IPressSystemCommands commands, int maxTimeSetPressureOperation)
        {
            MaxTimeSetPressureOperation = maxTimeSetPressureOperation;
            this.commands = commands;
        }

        public void Connect(CancellationToken cancellationToken)
        {
            Connect(CurrentOutputChannel, cancellationToken);
        }

        CancellationTokenSource cts;
        CancellationToken cycleReadCancellation;
        Task taskCycleRead;

        public void Connect(int outChannelNumber, CancellationToken cancellationToken)
        {
            try
            {
                ClearException();
                if (!ConnectState)
                {
                    BeginConnectEvent?.Invoke(this, new EventArgs());
                    commands.Connect(outChannelNumber, cancellationToken);
                    taskCycleRead = StartCycleRead();
                    ConnectState = true;
                }
                ConnectEvent?.Invoke(this, new EventArgs());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PressSystemException(ex.Message);
            }
        }

        public void Disconnect()
        {
            if (ConnectState)
            {
                cts.Cancel();
                if (taskCycleRead != null)
                    taskCycleRead.Wait();
                commands.Disconnect();
                ConnectState = false;
            }
            DisconnectEvent?.Invoke(this, new EventArgs());
        }

        AutoResetEvent updatePressureVarAutoReset = new AutoResetEvent(false);

        private async Task StartCycleRead()
        {
            cts = new CancellationTokenSource();
            cycleReadCancellation = cts.Token;
            try
            {
                await Task.Run(() => CycleReadVar());
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Exception = e;
                System.Diagnostics.Debug.WriteLine("Catch на StartCycleRead");
            }
        }

        private async Task CycleReadVar()
        {
            try
            {
                while (true)
                {
                    cycleReadCancellation.ThrowIfCancellationRequested();
                    PressSystemVariables = commands.ReadSysVar();
                    UpdateMeasures?.Invoke(this, new EventArgs());
                    updatePressureVarAutoReset?.Set();
                    await Task.Delay(100);
                    //throw new PressSystemException("Ошибка опроса");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Catch на CycleReadVar");
                throw new PressSystemException(ex.Message);

            }
        }


        public void ReadInfo()
        {
            try
            {
                Info = commands.ReadInfo();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PressSystemException(ex.Message);
            }
        }



        // Дожидается стабилизации давления
        private void WaitSetPessure(int maxOperationTime, CancellationToken cancellationToken)
        {
            using (System.Timers.Timer timer = new System.Timers.Timer(maxOperationTime * 1000))
            {
                bool timeout = false;
                timer.AutoReset = false;
                timer.Elapsed += (obj, e) => timeout = true;
                timer.Start();
                while (true)
                {
                    if (timeout)
                    {
                        Exception = new SetPressureTimeoutException();
                        throw Exception;
                    }

                    WaitUpdatePressureVar(cancellationToken);
                    if (CheckInlim(cancellationToken))
                        break;
                }
            }

        }

        // Параметр InLim должен быть true в течение времени DelayCheckInlim
        // Если так, то давление стабилизировалось
        private bool CheckInlim(CancellationToken cancellationToken)
        {
            bool result = false;
            updatePressureVarAutoReset.Reset();
            using (System.Timers.Timer timer = new System.Timers.Timer(DelayCheckInlim * 1000))
            {
                timer.AutoReset = false;
                timer.Elapsed += (obj, e) => result = true;
                timer.Start();
                while (!result)
                {
                    WaitUpdatePressureVar(cancellationToken);
                    if (!PressSystemVariables.InLim)
                        break;
                }
                return result;
            }
        }

        private void WaitUpdatePressureVar(CancellationToken cancellationToken)
        {
            updatePressureVarAutoReset.Reset();
            while (!updatePressureVarAutoReset.WaitOne(100))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            System.Diagnostics.Debug.WriteLine("Ожидание завершения установки давления");

        }

        public void DisableControl()
        {
            commands.DisableControl();
        }

        public void Dispose()
        {
            updatePressureVarAutoReset.Dispose();
        }

        public void WriteNewSP(int controller, double SP, CancellationToken cancellationToken)
        {

            WriteNewSP(controller, SP, RateIsMax, cancellationToken);
        }

        public void WriteNewSP(double SP, CancellationToken cancellationToken)
        {
            WriteNewSP(CurrentController, cancellationToken);
        }

        public void WriteNewSP(int controller, double SP, double rate, CancellationToken cancellationToken)
        {
            double sp = SP;
            try
            {
                if (sp > Info.RangeHi)
                    sp = Info.RangeHi;
                if (sp < Info.RangeLo)
                    sp = Info.RangeLo;
                commands.WriteSP(controller, sp, rate, cancellationToken);
                CurrentController = controller;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PressSystemException(ex.Message);
            }
        }

        public void WriteNewSP(double SP, double rangeMin, double rangeMax, double rate, CancellationToken cancellationToken)
        {
            int controller = Info.SearshController(SP, rangeMax, rangeMin);
            WriteNewSP(controller, SP, rate, cancellationToken);
        }

        public void WriteNewSP(double SP, double rangeMin, double rangeMax, CancellationToken cancellationToken)
        {
            int controller = Info.SearshController(SP, rangeMax, rangeMin);
            WriteNewSP(controller, SP, cancellationToken);
        }

        

        public void WriteNewSP(double SP, double rangeMin, double rangeMax, bool absolutePressure, CancellationToken cancellationToken)
        {
            double shift = 0;
            if (absolutePressure)
                shift = GetBarometricValue(cancellationToken);
            WriteNewSP(SP - shift, rangeMin - shift, rangeMax - shift, cancellationToken);
        }

        public void WriteNewSP(double SP, double rangeMin, double rangeMax, bool absolutePressure, double rate, CancellationToken cancellationToken)
        {
            double shift = 0;
            if (absolutePressure)
                shift = GetBarometricValue(cancellationToken);
            WriteNewSP(SP - shift, rangeMin - shift, rangeMax - shift, rate, cancellationToken);
        }

        public void SetPressure(double SP, CancellationToken cancellationToken)
        {
            SetPressure(CurrentController, SP, cancellationToken);
        }

        public void SetPressure(int controller, double SP, CancellationToken cancellationToken)
        {
            WriteNewSP(controller, SP, cancellationToken);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
        }

        public void SetPressure(int controller, double SP, int maxOperationTime, CancellationToken cancellationToken)
        {
            WriteNewSP(controller, SP, cancellationToken);
            WaitSetPessure(maxOperationTime, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, rangeMin, rangeMax, cancellationToken);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, bool absolutePressure, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, rangeMin, rangeMax, absolutePressure, cancellationToken);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, int maxOperationTime, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, rangeMin, rangeMax, cancellationToken);
            WaitSetPessure(maxOperationTime, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, double rate, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, rangeMin, rangeMax, rate, cancellationToken);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, bool absolutePressure, double rate, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, rangeMin, rangeMax, absolutePressure, rate, cancellationToken);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, double rate, int maxOperationTime, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, rangeMin, rangeMax, rate, cancellationToken);
            WaitSetPessure(maxOperationTime, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, bool absolutePressure, double rate, int maxOperationTime, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, rangeMin, rangeMax, absolutePressure, rate, cancellationToken);
            WaitSetPessure(maxOperationTime, cancellationToken);
        }


        private double GetBarometricValue(CancellationToken cancellationToken)
        {
            WaitUpdatePressureVar(cancellationToken);
            return PressSystemVariables.Barometr;
        }
    }
}
