using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressSystems;

namespace PressureSensorTestCore
{
    public class DigitalMeasurmentTools: AbstractMeasurmentTools
    {
        IDigitalPort digitalPort;

        public DigitalMeasurmentTools(IPressSystem psys, int outChannelPsys, IDigitalPort digitalPort): 
            base(psys, outChannelPsys)
        {
            this.digitalPort = digitalPort;
   
        }

        public override void Init()
        {
            digitalPort.Init();
        }

        public override ICheckPoint GetCheckPoint(double testPoint, double rangeMin, double rangeMax, bool absolutePressure, 
            PressureUnitsEnum pressureUnits, double classPrecision, double marginCoefficient, CancellationToken cancellationToken)
        {
            SetPressure(testPoint, rangeMin, rangeMax, absolutePressure, cancellationToken);

            bool zeroPressure = ((int)((rangeMax - rangeMin) * testPoint - rangeMin) == 0) && !absolutePressure;

            Measures(zeroPressure, out double referencePressure, out double pressure, absolutePressure, cancellationToken);

            ICheckPoint point = new DigitalCheckPoint((int)(testPoint * 100), rangeMin, rangeMax, referencePressure, pressure, classPrecision,
                pressureUnits, marginCoefficient);
            return point;
        }

        private void Measures(bool zeroPressure, out double referencePressure, out double pressure, bool absolutePressure, 
            CancellationToken cancellationToken)
        {
            double[] referencePressureItems = new double[NumberOfMeasurements];
            double[] pressureItems = new double[NumberOfMeasurements];
            for (int i = 0; i < NumberOfMeasurements; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Если пневмосистема не подключена, все порты стенда связаны с атмосферой
                // При этом давление на поверяемом преобразователе давления будет равно 0
                referencePressureItems[i] = (!zeroPressure) ? psys.PressSystemVariables.Pressure : 0;
                if (absolutePressure)
                    referencePressureItems[i] += psys.PressSystemVariables.Barometr;
                pressureItems[i] = GetDevicePressurePa();
                if (i < NumberOfMeasurements - 1)
                    Thread.Sleep(TimeoutBetwinMeasures * 1000);
            }
            referencePressure = referencePressureItems.Sum() / referencePressureItems.Length;
            pressure = Math.Round(pressureItems.Sum() / referencePressureItems.Length, 4, MidpointRounding.AwayFromZero);
        }

        private double GetDevicePressurePa()
        {
            try
            {
                return digitalPort.GetPressurePa();
            }
            catch
            {
                throw new LostConnectException();
            }
        }

        public override void Dispose()
        {
            digitalPort.Dispose();
        }
    }

    
}
