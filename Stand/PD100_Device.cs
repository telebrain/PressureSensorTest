using System;
using PressSystems;


namespace OwenPressureDevices
{
    public class PD100_Device : IDevice
    {
        //readonly IDeviceSpecification deviceSpecification;

        public PD100_Device()
        {
            SerialNumber = "";
            Name = "";
        }

        public PD100_Device(string serialNumber, string name)
        {
            SerialNumber = serialNumber;
            Name = name;
        }

        public string ChannelNumber { get; }

        public string SerialNumber { get; set; }

        public int MetrologicGroupNumber { get; set; }

        string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                try
                {
                    ComponentsOfDeviceName components = ParserNamePD100.ParseName(value);
                    Modification = components.Modification;
                    ThreadType = components.ThreadType;
                    ClassPrecision = Convert.ToSingle(components.Precision);
                    TargetPrecision = ClassPrecision > 0.5F ? 0.5F : ClassPrecision;
                    TargetVariation = TargetVariation / 2;
                    int range_Pa = ParserNamePD100.GetPressureRange(components.PressureRange);
                    RangeTypeEnum rangeType = (RangeTypeEnum)ParserNamePD100.RangeTypesLabels.IndexOf(components.RangeType);
                    Range = new DeviceRange(range_Pa, rangeType);
                    MetrologicGroupNumber = FindMetrologicGroupNumber(Range);
                }
                catch
                {
                    throw new ParseDeviceNameException();
                }
            }
        }
        // Модификация по типу сенсора
        public string Modification { get; private set; }
        // Тип резьбы 1 - 20х1,5, 7 - G1/2, 8 - G1/4 
        public string ThreadType { get; private set; }

        // Класс точности по паспорту
        public float ClassPrecision { get; private set; }
        // Класс точности нового преобразователя (только что изготовленного)
        public float TargetPrecision { get; private set; }

        // Вариация нового преобразователя (только что изготовленного)
        public float TargetVariation { get; private set; }

        public DeviceRange Range { get; private set; }

        // Код для Json протокола
         public int DeviceTypeCode { get { return 9; } }
        // Код для Json протокола
        public int SensorTypeCode { get { return 460; } }

        private int FindMetrologicGroupNumber(DeviceRange range)
        {
            // Пока заглушка. Нужно сделать класс MetrologicGroup и грузить группы из Excel
            if (range.Max_Pa <= 100000)
                return 6;
            return 5;
        }

    }

    public class ParseDeviceNameException: Exception
    {
        public ParseDeviceNameException():
            base("Не удалось определить изделие по названию")
        { }
    }
}
