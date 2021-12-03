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
            Name = null;
        }

        public PD100_Device(string serialNumber, string name): this(serialNumber, name, 0)
        {
        }

        public PD100_Device(string serialNumber, string name, int metrologicGroup):
            this (serialNumber, new DeviceName(name), metrologicGroup)
        {
        }

        public PD100_Device(string serialNumber, DeviceName name): this(serialNumber, name, 0) { }

        public PD100_Device(string serialNumber, DeviceName name, int metrologicGroup)
        {
            try
            {
                SerialNumber = serialNumber;
                Name = name;
                MetrologicGroupNumber = metrologicGroup;
                Range = new DeviceRange(GetRangePaByPressureLabel(Name.Range), Name.RangeType);
                Precision = Convert.ToSingle(Name.Precision);
                if (Name.Title == "ПД100")
                {
                    // У ПД100 любого класса точность должна быть не хуже 0.5
                    TargetPrecision = Precision > 0.5F ? 0.5F : Precision;
                }
                else
                {
                    TargetPrecision = Precision;
                }
                TargetVariation = TargetPrecision / 2;
                OutPort = OutPortEnum.Current;
                if (Name.OutPortLabel == DeviceName.RS485label || Name.OutPortLabel == "-" + DeviceName.RS485label)
                    OutPort = OutPortEnum.RS485;
            }
            catch
            {
                throw new ParseDeviceNameException();
            }
        }

        public string SerialNumber { get; set; }

        public int MetrologicGroupNumber { get; set; }

        public OutPortEnum OutPort { get; private set; }

        public DeviceName Name { get; }

        public float Precision { get; private set; }

        public float Variation { get; private set; }

        // Погрешность отбраковки при первичной поверке
        public float TargetPrecision { get; private set; }

        // Значение вариации отбраковки при первичной поверке
        public float TargetVariation { get; private set; }

        public DeviceRange Range { get; private set; }

        // Код для Json протокола
        public int DeviceTypeCode { get { return 9; } }
        // Код для Json протокола
        public int SensorTypeCode { get { return 460; } }

        private int GetRangePaByPressureLabel(string rangeLabel)
        {
            return (int)(Convert.ToDouble(rangeLabel) * 1000000);
        }
    }

    [Serializable]
    public class ParseDeviceNameException: Exception
    {
        public ParseDeviceNameException():
            base("Не удалось определить изделие по названию")
        { }
    }

    
}
