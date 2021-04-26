using System.Globalization;
using System;
using PressSystems;
using System.Text.RegularExpressions;

namespace PressureRack
{
    internal static class Parsing
    {
        internal static string ExtractStringParametr(string name, string str) // Извлекает значение параметра из строки
        {
            return (new Regex(string.Format(@"(?<={0})(\w+)(?=;)", name)).Match(str)).ToString();
            //string strOut = str;
            //int poz = strOut.IndexOf(name);
            //if (poz < 0)
            //{
            //    strOut = "";
            //    return strOut;
            //}
            //strOut = strOut.Substring(poz + name.Length);
            //string[] split = strOut.Split(new char[] { ';' });
            //strOut = split[0];
            //string currDecSepar = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;

            //return strOut;
        }

        internal static double ExtractDoubleParametr(string name, string str)
        {
            try
            {
                //string sData = ExtractStringParametr(name, str);
                string sData = (new Regex(string.Format(@"(?<={0})(\S+)(?=;)", name)).Match(str)).ToString();
                NumberFormatInfo provider = new NumberFormatInfo();
                provider.NumberDecimalSeparator = ".";
                double val = Convert.ToDouble(sData, provider);
                return val;
            }
            catch
            {
                throw new PressureRackException(3);
            }
            
        }

        internal static int ExtractIntParametr(string name, string str)
        {
            if (!ExtractLongParametr(name, str, out long res))
                throw new PressureRackException(3);
            return (int)res;
        }

        internal static long ExtractLongParametr(string name, string str)
        {
            if (!ExtractLongParametr(name, str, out long res))
                throw new PressureRackException(3);
            return res;
        }

        internal static PressControllerInfo GetPressControllerInfo(string str, int number)
        {
            var info = new PressControllerInfo()
            {
                IsEnabled = (ExtractStringParametr("STATE:", str) == "ON"),
                RangeLo = ExtractDoubleParametr("LO:", str) * 1000,
                RangeHi = ExtractDoubleParametr("HI:", str) * 1000,
                Precision = ExtractDoubleParametr("PREC:", str),
                SN = ExtractStringParametr("SN:", str),
                Number = number
            };
            return info;
        }

        internal static PressSystemInfo GetPressSystemInfo(string str)
        {
            try
            {
                if ((new Regex(@"(?<=INFO::)(.+)(?=INFO;)")).IsMatch(str))
                {
                    var info = new PressSystemInfo()
                    {
                        RangeLo = ExtractDoubleParametr("LOLIM:", str),
                        RangeHi = ExtractDoubleParametr("HILIM:", str),
                        Controllers = GetControllers(str)
                    };
                    return info;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        internal static PressControllersList GetControllers(string str)
        {
            var controllers = new PressControllersList();
            int i = 1;
            while (true)
            {
                string cntrlInfo = ((new Regex(string.Format(@"(?<=CNTRL{0}::)(.+)(?=CNTRL{0};)", i))).Match(str)).ToString();
                if (string.IsNullOrEmpty(cntrlInfo))
                    break;
                controllers.Add(GetPressControllerInfo(cntrlInfo, i));
                i++;
            }
            return controllers;
        }

        internal static void DecodeError(string str)
        {
            int[] shift = new int[] { 3, 6 };
            string[] names = new string[] { "ERR:", "ERR_PS:" };
            long result = 0;
            for (int i = 0; i < names.Length; i++)
            {

                if (ExtractLongParametr(names[i], str, out result))
                {
                    throw new PressureRackException((int)result + shift[i]);
                }
            }

            throw new PressureRackException(3);
        }

        private static bool ExtractLongParametr(string name, string str, out long result)
        {
            string sData = (new Regex(string.Format(@"(?<={0})(-?\d+)(?=;)", name)).Match(str)).ToString();
            return long.TryParse(sData, out result);
        }
    }
}
