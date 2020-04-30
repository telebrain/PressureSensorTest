using System;
using System.Threading;

namespace PressSystems
{
    public interface IPressSystem
    {
        // Все давления в Па

        int MaxTimeSetPressureOperation { get; set; }

        PressSystemInfo Info { get; }

        double Pressure { get; }

        double CurrentSP { get; }

        double Barometr { get; }

        int CurrentChannel { get; }

        int CurrentOutputChannel { get; }

        bool InLim { get; }

        bool ConnectState { get; }

        void ReadInfo();

        void Connect(CancellationToken cancellationToken);

        void Connect(int outChannelNumber, CancellationToken cancellationToken);

        void Disconnect();

        void WriteNewSP(int channel, double SP);

        void WriteNewSP(double SP);

        event EventHandler UpdateMeasures;

        event EventHandler ExceptionEvent;

        event EventHandler ConnectEvent;

        event EventHandler DisconnectEvent;

        Exception Exception { get; }

        void SetPressure(double SP, CancellationToken cancellationToken);

        void SetPressure(int channel, double SP, CancellationToken cancellationToken);

        void SetPressure(int channel, double SP, int maxOperationTime, CancellationToken cancellationToken);

        void SetPressure(double SP, double rangeMin, double rangeMax, double classPrecission,
            CancellationToken cancellationToken);

        void SetPressure(double SP, double rangeMin, double rangeMax, double classPrecission,
            int maxOperationTime, CancellationToken cancellationToken);
    }
}