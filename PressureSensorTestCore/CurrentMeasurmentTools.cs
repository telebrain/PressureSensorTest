using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressSystems;
using SDM_comm;

namespace PressureSensorTestCore
{
    public class CurrentMeasurmentTools : AbstractMeasurmentTools
    {
        readonly IAmmetr ammeter;
        AutoResetEvent updCurrentValueAutoReset;

        public CurrentMeasurmentTools(IPressSystem psys, int outChannelPsys, IAmmetr ammeter) : base(psys, outChannelPsys)
        {
            this.ammeter = ammeter;
            updCurrentValueAutoReset = new AutoResetEvent(false);
            ammeter.UpdMeasureResult += (obj, e) => updCurrentValueAutoReset.Set();
        }

        public override void Init() { }

        public override ICheckPoint GetCheckPoint(double testPoint, double rangeMin, double rangeMax, bool absolutePressure, 
            PressureUnitsEnum pressureUnits, double classPrecision, double marginCoefficient, CancellationToken cancellationToken)
        {
            SetPressure(testPoint, rangeMin, rangeMax, absolutePressure, cancellationToken);

            bool zeroPressure = ((int)((rangeMax - rangeMin) * testPoint - rangeMin) == 0) && ! absolutePressure;
            Measures(zeroPressure, out double pressure, out double current, absolutePressure, cancellationToken);
            // Если уставка равна нулю, то тестируемое изделие связано с атмосферой. Давление считаем равным нулю, 
            // кроме случая с абсолютным давлением
            CurrentCheckPoint point = new CurrentCheckPoint((int)(testPoint * 100), rangeMin, rangeMax, pressure, current, classPrecision,
                pressureUnits, marginCoefficient);
            return point;
        }

        private void Measures(bool zeroPressure, out double pressure, out double current, bool absolutePressure, 
            CancellationToken cancellationToken)
        {
            double[] pressureItems = new double[NumberOfMeasurements];
            double[] currentItems = new double[NumberOfMeasurements];
            for (int i = 0; i < NumberOfMeasurements; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Если пневмосистема не подключена, все порты стенда связаны с атмосферой
                // При этом давление на поверяемом преобразователе давления будет равно 0
                pressureItems[i] = (!zeroPressure) ? psys.PressSystemVariables.Pressure : 0;
                if (absolutePressure)
                    pressureItems[i] += psys.PressSystemVariables.Barometr;
                currentItems[i] = GetCurrent(cancellationToken);
                CheckCurrent(currentItems[i]);
                if (i < NumberOfMeasurements - 1)
                    Thread.Sleep(TimeoutBetwinMeasures * 1000);
            }
            pressure = pressureItems.Sum() / pressureItems.Length;
            current = Math.Round(currentItems.Sum() / pressureItems.Length, 4, MidpointRounding.AwayFromZero);
        }

        private void Measures(out double pressure, out double current, bool absolutePressure,
            CancellationToken cancellationToken)
        {
            Measures(false, out pressure, out current, absolutePressure, cancellationToken);
        }

        private double GetCurrent(CancellationToken cancellation)
        {
            long pressureTimeStamp = psys.PressSystemVariables.TimeStamp;
            do
            {
                // Ждем наступление события обновления показаний миллиамперметра
                while (!updCurrentValueAutoReset.WaitOne(100))
                {
                    cancellation.ThrowIfCancellationRequested();
                    updCurrentValueAutoReset.Reset();
                }
            }
            // Если метка времени больше, чем у давелния, выходим
            while (ammeter.Timestamp <= pressureTimeStamp);
            return ammeter.Current;
        }


        const double LoCurrentAlarmLimit = 3.5;
        const double HiCurrentAlarmLimit = 20.5;

        private void CheckCurrent(double current)
        {
            if (current < LoCurrentAlarmLimit)
                throw new LoCurrentAlarmException();
            if (current > HiCurrentAlarmLimit)
                throw new HiCurrentAlarmException();
        }

        public override void Dispose()
        {
            updCurrentValueAutoReset.Dispose();
        }
    }

    [Serializable]
    public class LoCurrentAlarmException : Exception { }
    [Serializable]
    public class HiCurrentAlarmException : Exception { }
}
