using System.Globalization;
using System;

namespace PressureRack
{
    internal static class Decoder
    {
        internal static string ExtractData(string name, string str) // Извлекает значение параметра из строки
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
            if (currDecSepar != ".")
            {
                strOut = strOut.Replace(".", currDecSepar);
            }
            return strOut;
        }

        internal static bool ExtractData(string name, string str, out double data)
        {
            data = 0;
            string sData = ExtractData(name, str);
            return double.TryParse(sData, out data);
        }

        internal static bool ExtractData(string name, string str, out int data)
        {
            data = 0;
            string sData = ExtractData(name, str);
            return Int32.TryParse(sData, out data);
        }

        internal static int DecodeError(string str)
        {
            int[] shift = new int[] { 3, 6 };
            string[] names = new string[] { "ERR:", "ERR_PS:" };
            int result = 0;
            for (int i = 0; i < names.Length; i++)
            {
                if (ExtractData(names[i], str, out result))
                {
                    return result + shift[i];
                }
            }

            return 3;
        }
    }
}
