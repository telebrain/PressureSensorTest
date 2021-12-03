using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OwenPressureDevices
{
    public class DeviceName
    {
        public string Name { get; }

        public string Title { get; }
        public string RangeType { get; }
        public string Range { get; }
        public string Modification { get; }
        public string Precision { get; }
        public string ThreadType { get; }
        public string Modification2 { get; }
        public string OutPortLabel { get; }


        public const string RS485label = "R";

        public DeviceName(string name)
        {
            Name = name;

            string[] splitName = name.Split(new char[] { '-' });


            // ПД100-ДИ0,25-111-0,5
            Title = splitName[0];

            // Поиск цифрового значения во второй части имени
            Regex regex = new Regex(string.Format(@"\d,\d|\d"));
            Match matchRange = regex.Match(splitName[1]);

            if (matchRange.Index <= 0)
                throw new ParseDeviceNameException();

            Range = splitName[1].Substring(matchRange.Index);
            RangeType = splitName[1].Substring(0, matchRange.Index);
            Modification = (splitName[2])[0].ToString();
            ThreadType = (splitName[2])[1].ToString();
            Modification2 = (splitName[2])[2].ToString();
            Precision = splitName[3];
            if (splitName.Length > 4)
                OutPortLabel = splitName[4];
            else
                OutPortLabel = "";     
        } 

        public DeviceName(string title, string rangeType, string range, string modification, string threadType, string modification2, 
            string precisiion, string outPortLabel)
        {
            Title = title;
            RangeType = rangeType;
            Range = range;
            Modification = modification;
            ThreadType = threadType;
            Modification2 = modification2;
            Precision = precisiion;
            OutPortLabel = outPortLabel;
            Name = $"{Title}-{RangeType}{Range}-{Modification}{ThreadType}{Modification2}-{Precision}";
            if (OutPortLabel == "")
                return;
            if (OutPortLabel[0] != '-')
                Name += "-";
            Name += OutPortLabel;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
