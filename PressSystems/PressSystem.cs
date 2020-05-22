﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PressSystems
{
    public class PressSystem : IPressSystem
    {

        public PressSystemInfo Info { get; private set; }

        public double CurrentSP { get; private set; }


        public int CurrentController { get; private set; }

        public int CurrentOutputChannel { get; private set; } = 1;

        public PressSystemVariables PressSystemVariables { get; private set; }

        public bool ConnectState { get; private set; }

        public int MaxTimeSetPressureOperation { get; set; } = 30; // В секундах

        const int DelayCheckInlim = 3; // в секундах

        Exception exception = null;
        public virtual Exception Exception
        {
            get { return exception; }
            private set
            {
                if (exception == null)
                {
                    exception = value;
                    ExceptionEvent(this, new EventArgs());
                }
            }
        }

        public event EventHandler UpdateMeasures;
        public event EventHandler ExceptionEvent;
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
                if (!ConnectState)
                {
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

        public virtual void Disconnect()
        {
            if (ConnectState)
            {
                cts.Cancel();
                if (taskCycleRead != null)
                    taskCycleRead.Wait();
                ConnectState = false;
            }
            DisconnectEvent?.Invoke(this, new EventArgs());
        }

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
                    await Task.Delay(100);
                    throw new PressSystemException("Ошибка опроса");
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

        public void SetPressure(double SP, CancellationToken cancellationToken)
        {
            WriteNewSP(SP, cancellationToken);
            WaitSetPessure(MaxTimeSetPressureOperation, cancellationToken);
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
            int controller = Info.SearshController(SP, rangeMax, rangeMin);
            SetPressure(controller, SP, cancellationToken);
        }

        public void SetPressure(double SP, double rangeMin, double rangeMax, int maxOperationTime, CancellationToken cancellationToken)
        {
            int controller = Info.SearshController(SP, rangeMax, rangeMin);
            SetPressure(controller, SP, maxOperationTime, cancellationToken);
        }

        public void WriteNewSP(int controller, double SP, CancellationToken cancellationToken)
        {
            try
            {
                commands.WriteSP(controller, SP, cancellationToken);
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

        public void WriteNewSP(double SP, CancellationToken cancellationToken)
        {
            WriteNewSP(CurrentController, SP, cancellationToken);
        }

        // Дожидаемся стабилизации давления
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

                    cancellationToken.ThrowIfCancellationRequested();
                    if (CheckInlim(cancellationToken))
                        break;
                    Thread.Sleep(200);
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
                while (PressSystemVariables.InLim)
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

        public void DisableControl()
        {
            commands.DisableControl();
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

    public class PsysSupportException : Exception
    {
        public PsysSupportException() :
            base("Пневмосистема не поддерживает поверку данного типа датчика")
        { }
    }
}