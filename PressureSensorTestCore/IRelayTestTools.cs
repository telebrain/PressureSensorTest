using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PressureSensorTestCore
{
    public interface IRelayTestTools
    {
        // int NumberOfRealays { get; }

        RelayTestPointUpDown[] GetTestPoints(RelayTestPointSettings settings, bool absolutePressure,
            PressureUnitsEnum pressureUnits, double marginCoefficient, CancellationToken cancellationToken);

    }
}
