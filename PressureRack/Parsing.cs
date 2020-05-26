using System.Globalization;
using System;
using PressSystems;

namespace PressureRack
{
    internal static class Parsing
    {
        internal static string ExtractStringParametr(string name, string str) // Извлекает значение параметра из строки
        {
            string strOut = str;
            int poz = strOut.IndexOf(name);
            if (poz < 0)
            {
                strOut = "";
                return strOut;
            }
            strOut = strOut.Substring(poz + name.Length);
            string[] split = strOut.Split(new char[] { ';' });
            strOut = split[0];
            string currDecSepar = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;

            return strOut;
        }

        internal static double ExtractDoubleParametr(string name, string str)
        {
            string sData = ExtractStringParametr(name, str);
            if (!double.TryParse(sData, NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"), out double val))
                throw new PressureRackException(3);
            return val;
        }

        internal static int ExtractIntParametr(string name, string str)
        {
            if (!ExtractIntParametr(name, str, out int res))
                throw new PressureRackException(3);
            return res;
        }

        internal static PressControllerInfo ExtractPressControllerInfo(string str)
        {
            var info = new PressControllerInfo()
            {
                IsEnabled = (ExtractStringParametr("STATE:", str) == "ON"),
                RangeLo = Convert.ToDouble(ExtractStringParametr("LO:", str))*1000,
                RangeHi = Convert.ToDouble(ExtractStringParametr("HI:", str))*1000,
                Precision = Convert.ToDouble(ExtractStringParametr("PREC:", str)),
                SN = Parsing.ExtractStringParametr("SN:", str)
            };
            return info;
        }

        

        internal static void DecodeError(string str)
        {
            int[] shift = new int[] { 3, 6 };
            string[] names = new string[] { "ERR:", "ERR_PS:" };
            int result = 0;
            for (int i = 0; i < names.Length; i++)
            {

                if (ExtractIntParametr(names[i], str, out result))
                {
                    throw new PressureRackException(result + shift[i]);
                }
            }

            throw new PressureRackException(3);
        }

        private static bool ExtractIntParametr(string name, string str, out int result)
        {
            string sData = ExtractStringParametr(name, str);
            return int.TryParse(sData, out result);
        }
    }
}
