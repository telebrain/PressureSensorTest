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
                if (value != "")
                {
                    ComponentsOfDeviceName components = ParserNamePD100.ParseName(value);
                    Modification = components.Modification;
                    ThreadType = components.ThreadType;
                    ClassPrecision = Convert.ToSingle(components.Precision);
                    int range_Pa = ParserNamePD100.GetPressureRange(components.PressureRange);
                    RangeTypeEnum rangeType = (RangeTypeEnum)ParserNamePD100.RangeTypesLabels.IndexOf(components.RangeType);
                    Range = new DeviceRange(range_Pa, rangeType);
                }
            }
        }

        public string Modification { get; private set; }

        public string ThreadType { get; private set; }

        public float ClassPrecision { get; private set; }

        public DeviceRange Range { get; private set; }

        // Пока как заглушка. Когда метрологи разберутся с формированием кодов, нужно написать реализацию
        public int DeviceTypeCode { get { return 3; } }
        // Тоже пока непонятно, какой ставить
        public int SensorTypeCode { get { return 2; } }
    }
}
