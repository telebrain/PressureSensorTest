using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PressSystems
{
    public abstract class AbstractPressSystem : IPressSystem
    {

        public PressSystemInfo Info { get; protected set; }

        public virtual double Pressure { get; protected set; }

        public virtual double CurrentSP { get; protected set; }

        public virtual double Barometr { get; protected set; }

        public int CurrentChannel { get; protected set; }

        public int CurrentOutputChannel { get; protected set; } = 1;

        public bool InLim { get; protected set; }

        public bool ConnectState { get; protected set; }

        public int MaxTimeSetPressureOperation { get; set; } = 30; // В секундах

        public int DelayCheckInlim { get; set; } = 3; // в секундах

        Exception exception = null;
        public virtual Exception Exception
        {
            get { return exception; }
            protected set
            {
                if (exception == null)
                {
                    exception = value;
                    ExceptionEvent(this, new EventArgs());
                }
            }
        }

        public virtual event EventHandler UpdateMeasures;
        public virtual event EventHandler ExceptionEvent;
        public virtual event EventHandler ConnectEvent;
        public virtual event EventHandler DisconnectEvent;

        public AbstractPressSystem(int maxTimeSetPressureOperation)
        {
            MaxTimeSetPressureOperation = maxTimeSetPressureOperation;
        }

        public void Connect(CancellationToken cancellationToken)
        {
            Connect(CurrentOutputChannel, cancellationToken);
        }

        CancellationTokenSource cts;
        CancellationToken cycleReadCancellation;
        Task taskCycleRead;

        protected abstract void ConnectOperation(int outChannelNumber, CancellationToken cancellationToken);

        public void Connect(int outChannelNumber, CancellationToken cancellationToken)
        {
            try
            {
                if (!ConnectState)
                {
                    ConnectOperation(outChannelNumber, cancellationToken);
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

        public virtual void Disconnect()
        {
            if (ConnectState)
            {
                cts.Cancel();
                if (taskCycleRead != null)
                    taskCycleRead.Wait();
                ConnectState = false;
                if(taskCycleRead != null)
                    taskCycleRead.Wait();
            }
            DisconnectEvent?.Invoke(this, new EventArgs());
        }

        protected async Task StartCycleRead()
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
            }
        }

        private void CycleReadVar()
        {
            try
            {
                Pressure = CurrentSP;
                while (true)
                {
                    cycleReadCancellation.ThrowIfCancellationRequested();
                    ReadSysVar();
                    UpdateMeasures?.Invoke(this, new EventArgs());
                }
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

        protected abstract void ReadSysVar();

        protected abstract void ReadInfoCore();

        public void ReadInfo()
        {
            try
            {
                ReadInfoCore();
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

        public void SetPressure(double SP, CancellationToken cancellationToken)
        {
            WriteNewSP(SP);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
        }

        public void SetPressure(int channel, double SP, CancellationToken cancellationToken)
        {
            WriteNewSP(channel, SP);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
        }

        public void SetPressure(int channel, double SP, int maxOperationTime, CancellationToken cancellationToken)
        {
            WriteNewSP(channel, SP);
            WaitSetPessure(maxOperationTime, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, double classPrecission, CancellationToken cancellationToken)
        {
            int channel = Info.SearshController(SP, rangeMax - rangeMin, classPrecission);
            SetPressure(channel, SP, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, double classPrecission, int maxOperationTime, CancellationToken cancellationToken)
        {
            int channel = Info.SearshController(SP, rangeMax - rangeMin, classPrecission);
            SetPressure(channel, SP, maxOperationTime, cancellationToken);
        }

        protected abstract void WriteSP(int channel, double SP);

        public void WriteNewSP(int channel, double SP)
        {
            try
            {
                WriteSP(channel, SP);
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

        public void WriteNewSP(double SP)
        {
            WriteNewSP(CurrentChannel, SP);
        }

        // Дожидаемся стабилизации давления
        protected virtual void WaitSetPessure(int maxOperationTime, CancellationToken cancellationToken)
        {
            bool timeOut = false;
            using (System.Timers.Timer timer = new System.Timers.Timer(maxOperationTime * 1000))
            {
                timer.AutoReset = false;
                timer.Elapsed += (obj, e) => timeOut = true;
                timer.Start();
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (Exception != null)
                        throw Exception;
                    if (timeOut)
                        break;
                    if (CheckInlim(cancellationToken))
                        break;
                    Thread.Sleep(200);
                }

                if (timeOut)
                {
                    Exception = new SetPressureTimeoutException();
                    throw Exception;
                }
            }
           
        }

        // Параметр InLim должен быть true в течение времени DelayCheckInlim
        // Если так, то давление стабилизировалось
        private bool CheckInlim(CancellationToken cancellationToken)
        {
            bool result = false;
            using (System.Timers.Timer timer = new System.Timers.Timer(DelayCheckInlim))
            {
                timer.AutoReset = false;
                timer.Elapsed += (obj, e) => result = true;
                timer.Start();
                while (InLim)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (Exception != null)
                        throw Exception;
                    if (result)
                        break;
                    Thread.Sleep(100);
                }
                return result;
            }
        }
    }

    public class SetPressureTimeoutException : Exception
    {
        public SetPressureTimeoutException() :
            base("Время установки давления превысило допустимое")
        { }
    }

    public class PressSystemException : Exception
    {
        public PressSystemException(string message) :
            base(message)
        { }
    }

    public class PsysPrecissionException : Exception
    {
        public PsysPrecissionException() :
            base("Пневмосистема не поддерживает поверку данного типа датчика")
        { }
    }
}
