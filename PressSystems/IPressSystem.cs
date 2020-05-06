using System;
using System.Threading;

namespace PressSystems
{
    public interface IPressSystem
    {
        // Все давления в Па

        // Максимально допустимое время установки давления, с
        int MaxTimeSetPressureOperation { get; set; }

        // Информация о пневмосистеме (ее диапазон, перечень контроллеров давления, их диапазон, класс точности)
        PressSystemInfo Info { get; }

        // Текущая уставка
        double CurrentSP { get; }

        // Давление
        double Pressure { get; }

        bool InLim { get; }

        // Показания барометра
        double Barometr { get; }

        // Метка времени обновления переменных
        long Timestamp { get; }

        // Номер контроллера давления, с которого производится задача давления в данный момент
        int CurrentController { get; }

        // Номер выхода (абонента) пневмосистемы
        int CurrentOutputChannel { get; }

        // Флаг выхода текущего контроллера на уставку
        

        // Флаг, позаывающий, что пневмосистема подключена к стенду и готова принимать уставку
        bool ConnectState { get; }

        // Команда чтения информации о пневмосистеме
        void ReadInfo();

        // Команда подключения текущего выхода пневмосистемы к стенду
        void Connect(CancellationToken cancellationToken);

        // Команда подключения выхода пневмосистемы к стенду
        void Connect(int outChannelNumber, CancellationToken cancellationToken);

        // Отключение пневмосистемы от стенда
        void Disconnect();

        // Запись уставки на один из контроллеров давления пневмосистемы
        void WriteNewSP(int controller, double SP, CancellationToken cancellationToken);

        // Запись уставки на текущий контроллер давления
        void WriteNewSP(double SP, CancellationToken cancellationToken);

        // Событие обновления измеренных параметров (давление, барометр, флаг выхода на уставку) 
        event EventHandler UpdateMeasures;

        // Событие, извещающее об аварии
        event EventHandler ExceptionEvent;

        // Событие подключения пневмосистемы к стенду
        event EventHandler ConnectEvent;

        // Событие откючения пневмосистемы от стенда
        event EventHandler DisconnectEvent;

        // Хранит последнее исключение (если null - рабочее состояние)
        Exception Exception { get; }

        // Установка давления с помощью текущего контроллера пневмосистемы, контроль выхода на уставку
        void SetPressure(double SP, CancellationToken cancellationToken);

        // Установка давления с помощью конкретного контроллера пневмосистемы, контроль выхода на уставку
        void SetPressure(int controller, double SP, CancellationToken cancellationToken);

        // Установка давления с помощью конкретного контроллера пневмосистемы, контроль выхода на уставку
        void SetPressure(int controller, double SP, int maxOperationTime, CancellationToken cancellationToken);

        // Установка давления с помощью контроллера пневмосистемы, определенного исходя 
        // из диапазона и класса точности калибруемого(тестируетого) изделия
        void SetPressure(double SP, double rangeMin, double rangeMax, double classPrecission,
            CancellationToken cancellationToken);

        // Установка давления с помощью контроллера пневмосистемы, определенного исходя 
        // из диапазона и класса точности калибруемого(тестируетого) изделия
        void SetPressure(double SP, double rangeMin, double rangeMax, double classPrecission,
            int maxOperationTime, CancellationToken cancellationToken);
    }
}