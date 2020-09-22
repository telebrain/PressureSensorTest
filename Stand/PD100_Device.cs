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
                    int range_Pa = ParserNamePD100.GetPressureRange(components.PressureRange);
                    RangeTypeEnum rangeType = (RangeTypeEnum)ParserNamePD100.RangeTypesLabels.IndexOf(components.RangeType);
                    Range = new DeviceRange(range_Pa, rangeType);
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

        public DeviceRange Range { get; private set; }

        // Пока как заглушка. Когда метрологи разберутся с формированием кодов, нужно написать реализацию
        public int DeviceTypeCode { get { return 3; } }
        // Тоже пока непонятно, какой ставить
        public int SensorTypeCode { get { return 2; } }
    }

    public class ParseDeviceNameException: Exception
    {
        public ParseDeviceNameException():
            base("Не удалось определить изделие по названию")
        { }
    }
}
