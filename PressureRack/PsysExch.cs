using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureRack
{
    public class PsysExch
    {
        readonly Exchange exch;
        readonly string ip;
        readonly int port;

        internal PsysExch(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            exch = new Exchange(this.ip, this.port);
        }

        // Чтение информации о всех контроллерах пневмосистемы
        internal DruckState[] GetDruckInfo() 
        {
            try
            {
                DruckState[] info = new DruckState[4];
                for (int i = 0; i < 4; i++)
                {
                    string tx = "INFO: " + Convert.ToString(i + 1) + ";";
                    string rx = "";
                    rx = exch.Exch(tx);
                    info[i] = new DruckState(rx);
                }
                return info;
            }
            catch { exch.Close(); return null; }
        }

        internal void GetPressureLimits(out double LoLimit, out double HiLimit)
        {
            string tx = "LIMITS?;";
            string rx;
            LoLimit = HiLimit = 0;

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

            if (!Decoder.ExtractData("LO:", rx, out LoLimit))
                throw new PressureRackException(3);

            if (!Decoder.ExtractData("HI:", rx, out HiLimit))
                throw new PressureRackException(3);
        }


        // Запрос подключения к пневмосистеме стойки
        internal void TryConnectPsys(int ch) 
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

        internal bool WaitConnectResult()
        {
            System.Diagnostics.Debug.WriteLine("Запрос состояния подключения");
            string tx = "STATECONN?;";
            string rx;
            try
            {
                rx = exch.Exch(tx);
                System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
            }
            catch
            {
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
        internal bool GetSysState()
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

            if (rx.IndexOf("SYS:REDY") >= 0) { return true; } // Операция завершена успешно
            if (rx.IndexOf("SYS:EXEC") >= 0) { return false; } // Операция еще не завершена

            int err = Decoder.DecodeError(rx);

            System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
            throw new PressureRackException(err);

        }

        // Чтение переменных
        internal void ReadVar(out double pv, out double bar, out bool inlim)
        {

            string rx = "";
            string tx = "VAR?";
            pv = 0;
            bar = 0;
            inlim = false;
            try
            {
                rx = exch.Exch(tx);
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("VAR") >= 0)
            {
                string[] names = new string[] { "PV:", "BAR:", "INLIM:" };
                string[] result = new string[names.Length];

                if (!Decoder.ExtractData("PV:", rx, out pv))
                        throw new PressureRackException(3);

                if (!Decoder.ExtractData("BAR:", rx, out bar))
                    throw new PressureRackException(3);

                if (!Decoder.ExtractData("INLIM:", rx, out int inlimRes))
                    throw new PressureRackException(3);

                inlim = inlimRes == 1;

                return;
            }

            int err = Decoder.DecodeError(rx);

            System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
            throw new PressureRackException(err);

        }

        // Передача уставки
        internal void SendSP(double sp, int ch)
        {
            string rx = "";
            string tx = "SP: VOL:" + DoubleValToString(sp) + ";" + "CH:" + Convert.ToString(ch) + ";";
            System.Diagnostics.Debug.WriteLine("Запись уставки: " + tx);
            try
            {
                rx = exch.Exch(tx);
                System.Diagnostics.Debug.WriteLine("Принятое сообщение: " + rx);
            }
            catch
            {
                throw new PressureRackException(2);
            }

            if (rx.IndexOf("OK") < 0)
            {
                int err = Decoder.DecodeError(rx);

                System.Diagnostics.Debug.WriteLine("Ошибка. Принятое сообщение: " + rx);
                throw new PressureRackException(err);
            }    
        }

        internal void Close()
        {
            if (exch != null)
            {
                exch.OnlySend("CLOSE;");
                exch.Close();
            }
        }

        private string DoubleValToString(double val)
        {
            string str = Convert.ToString(val);
            str = str.Replace(",", ".");
            return str;
        }
    }
}
