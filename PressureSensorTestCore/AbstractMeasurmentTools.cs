using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PressSystems;

namespace PressureSensorTestCore
{
    public abstract class AbstractMeasurmentTools: IMeasurmentTools
    {
        protected IPressSystem psys;
       
        public int NumberOfMeasurements { get; set; } = 1; 
        public int TimeoutBetwinMeasures { get; set; } = 1; // в секундах

        int outChannelPsys;

        public AbstractMeasurmentTools(IPressSystem psys, int outChannelPsys)
        {
            this.psys = psys;
            this.outChannelPsys = outChannelPsys;
        }

        public abstract void Init();

        protected void SetPressure(double testPoint, double rangeMin, double rangeMax, bool absolutePressure, 
            CancellationToken cancellationToken)
        {
            double _rangeMin = rangeMin;
            double _rangeMax = rangeMax;
            // Находим уставку по точке диапазона
            double SP = testPoint * (_rangeMax - _rangeMin) + _rangeMin;

            if (!psys.ConnectState && ((int)SP != 0 || absolutePressure))
            {
                // Если уставка отличается от 0 или это датчик абсолютного давления,
                // и пневмосистема еще не подключена, подключаем пневмосистему
                psys.Connect(outChannelPsys, cancellationToken);
            }
            if (psys.ConnectState)
            {
                psys.SetPressure(SP, _rangeMin, _rangeMax, absolutePressure, cancellationToken);
            }
        }

        

        public abstract ICheckPoint GetCheckPoint(double testPoint, double rangeMin, double rangeMax, bool absolutePressure,
            PressureUnitsEnum pressureUnits, double classPrecision, double marginCoefficient, CancellationToken cancellationToken);

        public abstract void Dispose();
    }
}
