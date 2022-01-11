using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PressureSensorTestCore
{
    public interface IMeasurmentTools: IDisposable
    {
        int NumberOfMeasurements { get; set; } // Количестов измерений
        int TimeoutBetwinMeasures { get; set; } // в секундах

        void Init();

        ICheckPoint GetCheckPoint(double testPoint, double rangeMin, double rangeMax, bool absolutePressure, PressureUnitsEnum pressureUnits,
            double classPrecision, double marginCoefficient, CancellationToken cancellationToken);
    }
}
