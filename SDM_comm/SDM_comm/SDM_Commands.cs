using System;
using System.Threading;
using System.Globalization;
using System.Text;

namespace SDM_comm
{
    public class SDM_Commands
    {
        readonly Transport transport = null;

        public SDM_Commands(Transport transport)
        {
            this.transport = transport;
            
        }

        public void InitCommand()
        {
            transport.OnlySend("INIT");
            Thread.Sleep(1000);
        }

        public void SetCurrentRange(CurrentType currentType, int value, CurrentUnitsEnum unit)
        {
            string send = "CONF:CURR:" + currentType + " ";
            send += (unit == CurrentUnitsEnum.AUTO) ? unit.ToString() : (value.ToString() + unit.ToString());
            transport.OnlySend(send);
            Thread.Sleep(1000);
            //transport.OnlySend("CONF?");
            for (int i = 0; i < 5; i++)
            {
                string config = WriteRead("CONF?");
                if (CheckSetCurrentRange(config, value, unit))
                    return;
                Thread.Sleep(1000);
            }

            throw new Exception("Не удалось установить режим измерения тока");
        }

        public double ReadMeasValue()
        {
            string val = WriteRead("READ?");
            val = val.Substring(0, val.Length - 3);
            return ConvertRecevedValue(val);
        }

        public double ReadMeasValue(int attempts)
        {
            try
            {
                double res = ReadMeasValue();
                return res;
            }
            catch (Exception ex)
            {
                attempts --;
                if (attempts <= 0)
                    throw ex;
                Thread.Sleep(1000);
                return ReadMeasValue(attempts);
            }
        }

        private string WriteRead(string send)
        {
            transport.OnlySend(send);
            Thread.Sleep(200);
            return transport.OnlyReceive();
        }

        private bool CheckSetCurrentRange(string config, int value, CurrentUnitsEnum unit)
        {
            try
            {
                config = config.Substring(0, config.Length - 4);
               
                const string key = "CURR ";
                if (config.IndexOf(key) < 0)
                    return false;

                if (unit != CurrentUnitsEnum.AUTO)
                {
                    long val = (long)(value * Math.Pow(10, (double)(unit - 1) * 3));
                    string rec = config.Substring(key.Length + config.LastIndexOf(key));
                    long receiveVal = (long)(ConvertRecevedValue(rec) * 1E+6);
                    return (val == receiveVal);
                }
                else
                {
                    // В этом случае посмотреть, что конфигурация правильно записалась нельзя - в зависимости от значения на входе
                    // предел измерений меняется
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private double ConvertRecevedValue(string val)
        {
            var format = new CultureInfo("en-US", false).NumberFormat;
            return Double.Parse(val, format);
        }

    }

    public enum CurrentUnitsEnum
    {
        AUTO = 0, uA, mA, A
    }

    public enum CurrentType
    {
        AC, DC
    }

    
}
