using System;
using System.Threading;

namespace PressureSensorTestCore
{
    public interface ITestProcess
    {
        TestResults TestResults { get; }
        int NumberOfMeasurements { get; set; }
        int TimeoutBetwinMeasures { get; set; }

        event EventHandler UpdResultsEvent;

        void RunProcess(double rangeMin, double rangeMax, double classPrecision, 
            int outChannelPsys, bool absoluteType, CancellationToken cancellation, IProgress<int> progress);
    }
}