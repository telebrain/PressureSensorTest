using System.Globalization;
using System;

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
            string sData = ExtractStringParametr(name, str);
            if (!int.TryParse(sData, out int val))
                throw new PressureRackException(3);
            return val;
        }

        private static bool ExtractIntParametr(string name, string str, out int result)
        {
            string sData = ExtractStringParametr(name, str);
            return int.TryParse(sData, out result);
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
    }
}
