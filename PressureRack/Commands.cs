using System;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace PressureRack
{
    public class Commands
    {
        Exchange exch;       
        readonly object syncRoot = new object();

        public Commands(string ip, int port)
        {          
            exch = new Exchange(ip, port);
        }

        public void ConnectTcp()
        {
            exch.ConnectTcp();
        }

        // Чтение информации о всех контроллерах пневмосистемы
        public ControllerState[] GetControllersState() 
        {
            try
            {
                ControllerState[] info = new ControllerState[4];
                for (int i = 0; i < 4; i++)
                {
                    string tx = "INFO: " + Convert.ToString(i + 1) + ";";
                    string rx = "";
                    rx = exch.Exch(tx);
                    info[i] = new ControllerState(rx);
                }
                return info;
            }
            catch
            {
                exch.Dispose();
                throw;
            }
        }

        public void GetPressureRange(out double loRange, out double hiRange)
        {
            string tx = "LIMITS?;";
            string rx;
            loRange = hiRange = 0;
            try
            {
                System.Diagnostics.Debug.WriteLine("Запрос данных о границах пневмосистемы");
                rx = exch.Exch(tx);
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("LIMITS:") < 0)
            {
                throw new PressureRackException(1);
            }

            loRange = Parsing.ExtractDoubleParametr("LO:", rx);
            loRange = Parsing.ExtractDoubleParametr("HI:", rx);
        }

        public void Connect(int outChannelNumber, CancellationToken token)
        {
            TryConnectPsys(outChannelNumber);
            WaitConnect(token).GetAwaiter().GetResult();
        }

        // Запрос подключения к пневмосистеме стойки
        private void TryConnectPsys(int ch) 
        {
            string tx = "CONN:" + Convert.ToString(ch) + ";";
            string rx;
            try
            {
                System.Diagnostics.Debug.WriteLine("Попытка коннекта с пневмосистемой");
                rx = exch.Exch(tx);
                if (rx.IndexOf("OK") < 0)
                {
                    throw new Exception();
                }
            }
            catch
            {
                throw new PressureRackException(1);
            }
        }

        private async Task WaitConnect(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (CheckConnectResult())
                    break;
                await Task.Delay(500);
            }
        }

        private bool CheckConnectResult()
        {
            System.Diagnostics.Debug.WriteLine("Запрос состояния подключения");
            string tx = "STATECONN?;";
            string rx = "";
            try
            {
                lock (syncRoot)
                {
                    rx = exch.Exch(tx);
                }
                
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("WAIT") >= 0)
                return false;

            if (rx.IndexOf("CONN") >= 0)
                return true;

            System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
            throw new PressureRackException(2);
        }

        // Чтение состояния пневмосистемы
        private bool GetSysState()
        {
            string rx = "";
            string tx = "SYSSTATE?";
            try
            {
                rx = exch.Exch(tx);
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("SYS:REDY") >= 0)
                return true; // Операция завершена успешно
            if (rx.IndexOf("SYS:EXEC") >= 0)
                return false; // Операция еще не завершена

            System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
            Parsing.DecodeError(rx);
            return false;
        }

        // Чтение переменных

        PressureRackVariables pressureRackVariables = new PressureRackVariables();
        public PressureRackVariables ReadVar()
        {
            
            string rx = "";
            string tx = "VAR?";
            try
            {
                lock (syncRoot)
                {
                    rx = exch.Exch(tx);
                }
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("VAR") >= 0)
            {
                pressureRackVariables.Pressure = Parsing.ExtractDoubleParametr("PV:", rx);
                pressureRackVariables.Barometr = Parsing.ExtractDoubleParametr("BAR:", rx);
                pressureRackVariables.InLim = Parsing.ExtractDoubleParametr("INLIM:", rx) == 1;
                pressureRackVariables.TimeStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                return pressureRackVariables;
            }

            System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
            Parsing.DecodeError(rx);
            return pressureRackVariables;
        }

        public void WriteSP(int controller, double SP, CancellationToken cancellationToken)
        {
            SendSP(SP, controller);
            WaitResultSendSP(cancellationToken).GetAwaiter().GetResult();
        }

        // Передача уставки
        private void SendSP(double sp, int ch)
        {
            string rx = "";
            string tx = "SP: VOL:" + sp.ToString(new CultureInfo("en-En")) + ";" + "CH:" + Convert.ToString(ch) + ";";
            System.Diagnostics.Debug.WriteLine("Запись уставки: " + tx);
            try
            {
                lock(syncRoot)
                {
                    rx = exch.Exch(tx);
                }
                System.Diagnostics.Debug.WriteLine("Принятое сообщение: " + rx);
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("OK") < 0)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
                Parsing.DecodeError(rx);
            }    
        }

        private async Task WaitResultSendSP(CancellationToken token)
        {
            bool result = false;
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                lock (syncRoot)
                {
                    result = GetSysState();
                }
                if (result)
                    break;
                await Task.Delay(500);
            }
        }

        public void DisableControl()
        {
            string rx = "";
            string tx = "OFFCNTRL;";
            System.Diagnostics.Debug.WriteLine("Запись уставки: " + tx);
            try
            {
                lock (syncRoot)
                {
                    rx = exch.Exch(tx);
                }
                System.Diagnostics.Debug.WriteLine("Принятое сообщение: " + rx);
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("OK") < 0)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
                Parsing.DecodeError(rx);
            }
        }

        public void Close()
        {
            if (exch != null)
            {
                exch.OnlySend("CLOSE;");
                exch.Dispose();
            }
        }
    }
}
