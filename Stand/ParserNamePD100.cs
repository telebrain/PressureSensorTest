using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OwenPressureDevices
{
    public static class ParserNamePD100
    {
        public static List<string> Titles { get; } = new List<string>() { "ПД100", "ПД100И" };

        public static List<string> RangeTypesLabels { get; } = new List<string>() { "ДИ", "ДВ", "ДИВ", "ДГ", "ДА" };

        public static List<string> Modifications { get; } = new List<string>() { "1", "3", "8" };

        public static List<string> ThreadType { get; } = new List<string>() { "1", "7", "8" };

        public static List<string> Classes { get; } = new List<string>() { "0,25", "0,5", "1,0", "1,5", "2,5" };

        public static ComponentsOfDeviceName ParseName(string name)
        {
            //ComponentsOfDeviceName components = new ComponentsOfDeviceName();

            string[] splitName = name.Split(new char[] { '-' });


            // ПД100-ДИ0,25-111-0,5
            string title = splitName[0];

            // Поиск цифрового значения во второй части имени
            Regex regex = new Regex(string.Format(@"\d,\d"));
            Match matchRange = regex.Match(splitName[1]);
            
            if (matchRange.Index <= 0)
                CreateParseException();
            string range = splitName[1].Substring(matchRange.Index);
            string rangeType = splitName[1].Substring(0, matchRange.Index);
            string modif = splitName[2];
            string precision = splitName[3];

            ComponentsOfDeviceName nameItems = new ComponentsOfDeviceName(title, rangeType, range,  
                modif[0].ToString(), modif[1].ToString(), precision);
            return nameItems;
        }
        
        public static string ConcateName(ComponentsOfDeviceName nameComponents)
        {
            string name = String.Format("{0}-{1}{2}-{3}{4}X-{5}", nameComponents.Title, nameComponents.RangeType, nameComponents.PressureRange,
                nameComponents.Modification, nameComponents.ThreadType, nameComponents.Precision);
            return name;
        }

        public static string GetPressLabel(int PressValue_Pa)
        {
            double range = Convert.ToDouble(PressValue_Pa)/ 1000000;
            string lbl = range.ToString();
            if (lbl.Length == 1)
                lbl = range.ToString("0.0");
            return lbl;
        }

        public static int GetPressureRange(string rangeLabel)
        {
            return (int)(Convert.ToDouble(rangeLabel) * 1000000);
        }

        private static void CreateParseException()
        {

        }
    }

    
}
