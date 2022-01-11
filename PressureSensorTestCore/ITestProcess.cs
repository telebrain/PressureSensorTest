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

        void RunProcess(double rangeMin_Pa, double rangeMax_Pa, PressureUnitsEnum pressureUnits, double classPrecision, 
            bool absoluteType, CancellationToken cancellation, IProgress<int> progress);
    }

   
}