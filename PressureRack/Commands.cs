using System;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using PressSystems;

namespace PressureRack
{
    public class Commands: IPressSystemCommands
    {
        Exchange exchange;
        readonly object syncRoot = new object();

        public Commands(string ip, int port)
        {
            exchange = new Exchange(ip, port);
        }

        

        public void Connect(int outChannelNumber, CancellationToken cancellationToken)
        {
            exchange.ConnectTcp();
            TryConnectPsys(outChannelNumber);
            WaitConnect(cancellationToken).GetAwaiter().GetResult();
        }

        public void Disconnect()
        {
            exchange.OnlySend("CLOSE;");
            exchange.Dispose();
        }

        public PressSystemInfo ReadInfo()
        {
            exchange.ConnectTcp();

            PressSystemInfo info = new PressSystemInfo();
            info.Controllers = new PressControllersList();
            for (int i = 0; i < 4; i++)
            {
                info.Controllers.Add(GetControllerInfo(i + 1));
            }

            GetPressureRange(out double PressureLo, out double PressureHi);

            info.RangeLo = PressureLo;
            info.RangeHi = PressureHi;

            Disconnect();
            return info;
        }

        PressSystemVariables variables = new PressSystemVariables();
        public PressSystemVariables ReadSysVar()
        {

            string rx = "";
            string tx = "VAR?";
            try
            {
                lock (syncRoot)
                {
                    rx = exchange.Exch(tx);
                }
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("VAR") >= 0)
            {
                variables.Pressure = Parsing.ExtractDoubleParametr("PV:", rx) * 1000;
                variables.Barometr = Parsing.ExtractDoubleParametr("BAR:", rx) * 1000;
                variables.InLim = Parsing.ExtractDoubleParametr("INLIM:", rx) == 1;
                variables.TimeStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
                Parsing.DecodeError(rx);
            }
            return variables;
        }


        public void WriteSP(int controller, double SP, CancellationToken cancellationToken)
        {
            SendSP(SP, controller);
            WaitResultSendSP(cancellationToken).GetAwaiter().GetResult();
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
                    rx = exchange.Exch(tx);
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

        // Чтение информации о контроллере давления пневмосистемы
        private PressControllerInfo GetControllerInfo(int controller)
        {
            try
            {
                string tx = "INFO: " + Convert.ToString(controller) + ";";
                string rx = "";
                rx = exchange.Exch(tx);
                var info = Parsing.ExtractPressControllerInfo(rx);
                info.Number = controller;
                return info;
            }
            catch
            {
                exchange.Dispose();
                throw;
            }
        }

        private void GetPressureRange(out double loRange, out double hiRange)
        {
            string tx = "LIMITS?;";
            string rx;
            loRange = hiRange = 0;
            try
            {
                System.Diagnostics.Debug.WriteLine("Запрос данных о границах пневмосистемы");
                rx = exchange.Exch(tx);
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("LIMITS:") < 0)
            {
                throw new PressureRackException(1);
            }

            loRange = Parsing.ExtractDoubleParametr("LO:", rx)*1000;
            loRange = Parsing.ExtractDoubleParametr("HI:", rx)*1000;
        }

        // Запрос подключения к пневмосистеме стойки
        private void TryConnectPsys(int ch)
        {
            string tx = "CONN:" + Convert.ToString(ch) + ";";
            string rx;
            try
            {
                System.Diagnostics.Debug.WriteLine("Попытка коннекта с пневмосистемой");
                rx = exchange.Exch(tx);
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
                    rx = exchange.Exch(tx);
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

        // Передача уставки
        private void SendSP(double sp, int ch)
        {
            string rx = "";
            string tx = "SP: VOL:" + (sp*0.001).ToString(new CultureInfo("en-En")) + ";" + "CH:" + Convert.ToString(ch) + ";";
            System.Diagnostics.Debug.WriteLine("Запись уставки: " + tx);
            try
            {
                lock (syncRoot)
                {
                    rx = exchange.Exch(tx);
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


        // Чтение состояния пневмосистемы
        private bool GetSysState()
        {
            string rx = "";
            string tx = "SYSSTATE?";
            try
            {
                rx = exchange.Exch(tx);
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
    }

    
}
