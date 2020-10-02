using PressSystems;

namespace OwenPressureDevices
{
    public interface IDevice
    {
        string ChannelNumber { get; } // Номер канала на стенде

        // Начальные параметры калибровки или тестирования
        
        string SerialNumber { get; set; }
        
        string Name { get; set; }

        float ClassPrecision { get; }

        float TargetPrecision { get; }

        DeviceRange Range { get; }

        // Метрологическая группа (для Json протокола) 
        int MetrologicGroupNumber { get; set; }

        string Modification { get; }

        // Группа изделий (для Json протокола) 
        int DeviceTypeCode { get; }
        // Код сенсора (для Json протокола) 
        int SensorTypeCode { get; }
        // Тип резьбы
        string ThreadType { get; }

    }
}