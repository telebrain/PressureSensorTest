using System;
using System.Threading;

namespace SDM_comm
{
    public interface IAmmetr
    {
        double Current { get; }

        long Timestamp { get; }

        Exception Exception { get; }

        CurrentTypeEnum CurrentType { get; }

        CurrentUnitsEnum Units { get; }

        int Range { get; }

        event EventHandler<double> UpdMeasureResult;

        event EventHandler ExceptionEvent;

        event EventHandler ConnectEvent;

        event EventHandler DisconnectEvent;

        bool StateConnect { get; }

        void StartCycleMeasureCurrent();

        void Stop();
    }

    public class AmmetrErrException: Exception
    {
        public AmmetrErrException(string message): base(message) { }
    }
}