using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;


namespace JE_PACE
{
    public class Pace: IDisposable
    {
        IExchange exchange = null;

        public PaceUnitsEnum[] Units { get; private set; } = new PaceUnitsEnum[]
            {PaceUnitsEnum.NotDefined, PaceUnitsEnum.NotDefined};

        public Pace(IExchange exchange)
        {
            this.exchange = exchange;
        }

        public void Connect()
        {
            try
            {
                exchange.Connect();
            }
            catch (Exception ex)
            {
                throw new PaceExchException("ConnectError", ex);
            }
        }

        public void Disconnect()
        {
            exchange.Disconnect();
        }

        public string GetSn()
        {
            try
            {
                string received = exchange.Request(":INST:SN1?");
                return ParseStringValue(" ", received);
            }
            catch (Exception ex)
            {
                throw new PaceExchException("ReadSnError", ex);
            }
        }

        public string GetModulSn(int channel = 0)
        {
            try
            {
                string received = exchange.Request($":INST:SN{channel + 2}?");
                return ParseStringValue(" ", received);
            }
            catch (Exception ex)
            {
                throw new PaceExchException("ReadSnModulError", ex, channel);
            }
        }

        public bool CheckBarometr(int channel = 0)
        {
            try
            {
                string req = AddChannelToCommandStr(":INST:CAT{0}:ALL?", channel);
                string response = exchange.Request(req);
                return response.IndexOf("BAROMETER") >= 0;
            }
            catch (Exception ex)
            {
                throw new PaceExchException("ReadConfigError", ex);
            }
        }

        // Диапазон и занчения пределов всегда в паскалях
        public double GetRange(out double loLimitValue, out double hiLimitValue, int channel = 0)
        {
            try
            {
                double[] rangeValues = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    string req = AddChannelToCommandStr(":INST:CONT{0}:LIM", channel);
                    var response = exchange.Request(req + (i + 1).ToString() + "?");
                    rangeValues[i] = ParseRangeValue(response, i + 1);
                }
                loLimitValue = rangeValues[1];
                hiLimitValue = rangeValues[2];
                return rangeValues[0];
            }
            catch(Exception ex)
            {
                throw new PaceExchException("ReadRangeError", ex, channel);
            }
        }

        private double ParseRangeValue(string message, int index)
        {
            string parametr = index == 1 ? "LIM \"" : $"LIM{index} \"";
            string strValue = (new Regex(string.Format(@"(?<={0})(\w+)", parametr))).Match(message).ToString();
            if (!double.TryParse(strValue, out double value))
                throw new ParseException();
            value = message.IndexOf("mbar") < 0 ? value * 100000 : value * 100;  
            return value;
        }

        // Обозначение единиц измерения
        //ATM atmosphere
        //BAR bar
        //CMH2O centimetres of water at 20°C
        //CMHG centimetres of mercury
        //FTH2O feet of water at 20°C
        //FTH2O4 feet of water at 4°C
        //HPA hecto Pascals
        //INH2O inches of water at 20°C
        //INH2O4 inches of water at 4°C
        //INH2O60 inches of water at 60°F
        //INHG inches of mercury
        //KG/CM2 kilogrammes per square centimetre
        //KG/M2 kilogrammes per square metre
        //KPA kilo Pascals
        //LB/FT2 pounds per square foot
        //MH2O metres of water
        //MHG metres of mercury
        //MMH2O millimetres of water
        //MMHG millimetres of mercury
        //MPA mega Pascals
        //PA Pascals
        //PSI pounds per square inch
        //TORR torr
        //MBAR millibar

        private void WriteUnit(string unit, int channel = 0)
        {
            try
            {
                string req = AddChannelToCommandStr(":UNIT{0}:PRES ", channel);
                req += unit;
                exchange.Send(req);
            }
            catch(Exception ex)
            {
                throw new PaceExchException("WriteUnitError", ex, channel);
            }
        }

        public PaceUnitsEnum ReadUnit(int channel = 0)
        {
            try
            {
                string req = AddChannelToCommandStr(":UNIT{0}?", channel);
                string received = exchange.Request(req);
                string parametr = AddChannelToCommandStr(":UNIT{0}:PRES ", channel);
                string strValue = ParseStringValue(parametr, received);
                PaceUnitsEnum result = PaceUnitsEnum.NotDefined;
                for (int i = 0; i < 3; i++)
                {
                    if (((PaceUnitsEnum)i).ToString() == strValue)
                    {
                        result = (PaceUnitsEnum)i;
                        break;                      
                    }
                }
                Units[channel] = result;
                return result;
            }
            catch(Exception ex)
            {
                throw new PaceExchException("ReadUnitError", ex, channel);
            }
        }

        public void WriteUnitWithCheck(PaceUnitsEnum unit, int channel = 0)
        {
            WriteUnit(unit.ToString(), channel);
            var readUnit = ReadUnit(channel);
            if (readUnit != unit)
                throw new PaceExchException("WriteUnitError", channel);
        }

        private string ParseStringValue(string parametr, string message)
        {
            string val = (new Regex(string.Format(@"(?<={0})(\w+)", parametr))).Match(message).ToString();
            if (val == "")
                throw new ParseException();
            return val;
        }

        private void WriteSP(double sp, int channel = 0)
        {
            try
            {
                string req = AddChannelToCommandStr(":SOUR{0} ", channel);
                req += sp.ToString(new CultureInfo("en-us"));
                exchange.Send(req);
            }
            catch(Exception ex)
            {
                throw new PaceExchException("WriteSpError", ex, channel);
            }
        }

        private double ReadSP(int channel = 0)
        {
            try
            {
                string req = AddChannelToCommandStr(":SOUR{0}?", channel);
                string received = exchange.Request(req);
                return ParseDoubleVal(" ", received);
            }
            catch(Exception ex)
            {
                throw new PaceExchException("ReadSpError", ex, channel);
            }
        }

        // Уставка передается всегда в Паскалях
        public void WriteSpWithCheck(int sp_Pa, int channel = 0)
        {
            double spBySend = ConvertPressureBySend(sp_Pa, channel);
            WriteSP(spBySend, channel);
            Thread.Sleep(50);
            double readSP = GetPressurePaFromReceive(ReadSP(channel));
            if (((int)readSP) != sp_Pa)
                throw new PaceExchException("WriteSpError", channel);
        }

        public double ReadPressure(int channel = 0)
        {
            try
            {
                return ReadPressureParametr(":SENS{0}", channel);
            }
            catch(Exception ex)
            {
                throw new PaceExchException("ReadPressureError", ex, channel);
            }
        }

        public double ReadSourcePlus(int channel = 0)
        {
            try
            {
                return ReadPressureParametr(":SOUR{0}:COMP", channel);
            }
            catch (Exception ex)
            {
                throw new PaceExchException("ReadSrcPlusError", ex, channel);
            }
        }

        public double ReadSourceMinus(int channel = 0)
        {
            try
            {
                return ReadPressureParametr(":SOUR{0}:COMP2", channel);
            }
            catch (Exception ex)
            {
                throw new PaceExchException("ReadSrcMinusError", ex, channel);
            }
        }

        private void WriteOutState(bool enable, int channel = 0)
        {
            string req = AddChannelToCommandStr("OUTP{0} ", channel);
            req = enable ? req + "1" : req + "0";
            exchange.Send(req);
        }

        public bool ReadOutState(int channel = 0)
        {
            try
            {
                string command = AddChannelToCommandStr(":OUTP{0}:STAT?", channel);
                string response = exchange.Request(command);
                string[] items = response.Split(' ');
                if (!int.TryParse(items[1], out int state))
                    throw new ParseException();
                return state == 1;
            }
            catch(Exception ex)
            {
                throw new PaceExchException("ReadOutStateError", ex, channel);
            }
        }

        public void WriteOutStateWithCheck(bool enable, int channel = 0)
        {
            try
            {
                WriteOutState(enable, channel);
                bool state = ReadOutState(channel);
                if (state ^ enable)
                    throw new Exception();
            }
            catch(Exception ex)
            {
                throw new PaceExchException("WriteOutStateError", ex, channel);
            }
            
        }

        private string AddChannelToCommandStr(string command, int channel)
        {
            string insert = (channel == 0) ? "" : "2";
            return string.Format(command, insert);
        }

        private double ReadPressureParametr(string command, int channel)
        {
            string parametr = AddChannelToCommandStr(command, channel);
            string req = parametr + "?";
            string received = exchange.Request(req);
            double readVal = ParseDoubleVal(" ", received);
            return GetPressurePaFromReceive(readVal, channel);
        }

        private double ParseDoubleVal(string parametr, string message)
        {
            string[] strVals = message.Split(' ');
            string strVal = strVals[1];
            if (!double.TryParse(strVal, NumberStyles.Number, new CultureInfo("en-US"), out double value))
                throw new ParseException();
            return value;
        }

        private double ConvertPressureBySend(double pressure_Pa, int channel = 0)
        {
            if (Units[channel] == PaceUnitsEnum.NotDefined)
                ReadUnit(channel);
            switch(Units[channel])
            {
                case PaceUnitsEnum.MPA:
                    return pressure_Pa / 1000000;
                case PaceUnitsEnum.KPA:
                    return pressure_Pa / 1000;
                default:
                    return pressure_Pa;
            }
        }

        private double GetPressurePaFromReceive(double pressure, int channel = 0)
        {
            if (Units[channel] == PaceUnitsEnum.NotDefined)
                ReadUnit(channel);
            switch (Units[channel])
            {
                case PaceUnitsEnum.MPA:
                    return pressure * 1000000;
                case PaceUnitsEnum.KPA:
                    return pressure * 1000;
                default:
                    return pressure;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }

    

    public class PaceExchException : Exception
    {
        public PaceExchException(string keyMessage, Exception ex):
            base(Properties.Resources.ResourceManager.GetString(keyMessage) + ": " + ex.Message)
        {}

        public PaceExchException(string keyMessage, Exception ex, int channel) :
            base(string.Format(Properties.Resources.ResourceManager.GetString(keyMessage), channel + 1) + ": " + ex.Message)
        { }

        public PaceExchException(string keyMessage, int channel) :
            base(string.Format(Properties.Resources.ResourceManager.GetString(keyMessage), channel + 1))
        { }
    }

    public class ParseException: Exception
    {
        public ParseException() : base(Properties.Resources.ResourceManager.GetString("ParseError")) { }
    }


    public enum PaceUnitsEnum
    {
        NotDefined = -1, PA = 0, KPA, MPA
    }

    
}
