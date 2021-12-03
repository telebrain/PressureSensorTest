using PressSystems;

namespace OwenPressureDevices
{
    public interface IDevice
    {

        // Начальные параметры калибровки или тестирования
        
        string SerialNumber { get; set; }
        
        DeviceName Name { get; }

        float Precision { get; }

        float TargetPrecision { get; }

        DeviceRange Range { get; }

        OutPortEnum OutPort { get; }

        // Метрологическая группа (для Json протокола) 
        int MetrologicGroupNumber { get; set; }

        // Группа изделий (для Json протокола) 
        int DeviceTypeCode { get; }
        // Код сенсора (для Json протокола) 
        int SensorTypeCode { get; }

    }

    public enum OutPortEnum { Current = 0, Voltage = 1, RS485 = 2 }
}