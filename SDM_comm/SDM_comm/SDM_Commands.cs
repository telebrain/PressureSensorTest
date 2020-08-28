using System;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            transport.Send("INIT");
            //Pause(1000);
        }

        public void SetCurrentRange(CurrentTypeEnum currentType, int value, CurrentUnitsEnum unit)
        {
            string send = "CONF:CURR:" + currentType + " ";
            send += (unit == CurrentUnitsEnum.AUTO) ? unit.ToString() : (value.ToString() + unit.ToString());
            transport.Send(send);

            for (int i = 0; i < 5; i++)
            {
                Pause(300);
                string config = WriteRead("CONF?");
                if (CheckSetCurrentRange(config, value, unit))
                    return;                
            }

            throw new SDM_ErrException(Properties.Resources.SetCurrentMeasureModeError);
        }


        public void SetSamples(int samples)
        {
            transport.Send($"SAMP:COUNT {samples}");            
            Pause(2000);
            string rec = WriteRead("SAMP:COUNT?");
            string recVal = (new Regex(@"(?<=\+)(\w+)(?=\n)").Match(rec)).ToString();
            if (!(int.TryParse(recVal, out int val) && samples == val))
                throw new SDM_ErrException(Properties.Resources.SetSamples);           
        }

        public double ReadMeasValue()
        {
            string val = WriteRead("READ?");
            val = val.Substring(0, val.IndexOf('\n'));
            // Console.WriteLine(val);
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
                Pause(1000);
                return ReadMeasValue(attempts);
            }
        }

        private string WriteRead(string send)
        {
            try
            {
                transport.Send(send);
                Pause(200);
                return transport.Receive();
            }
            catch
            {
                throw new SDM_ErrException(Properties.Resources.ExchangeError);
            }
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

        private void Pause(int pause)
        {
            (Task.Run(async () => await Task.Delay(pause))).GetAwaiter().GetResult();
        }


        readonly NumberFormatInfo format = new CultureInfo("en-US", false).NumberFormat;
        private double ConvertRecevedValue(string val)
        {
            
            return Double.Parse(val, format);
        }

    }

    public enum CurrentUnitsEnum
    {
        AUTO = 0, uA, mA, A
    }

    public enum CurrentTypeEnum
    {
        AC, DC
    }

    
}
