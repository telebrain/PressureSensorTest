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

        DeviceRange Range { get; }

        string Modification { get; }

        // Метрологическая группа
        int DeviceTypeCode { get; }
        // Код сенсора
        int SensorTypeCode { get; }
        // Тип резьбы
        string ThreadType { get; }

    }
}