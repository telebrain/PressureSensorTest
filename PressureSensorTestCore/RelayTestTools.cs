using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressSystems;

namespace PressureSensorTestCore
{
    public class RelayTestTools: IRelayTestTools
    {
        IPressSystem psys;
        readonly int outChannelPsys;
        IStateRelayReader stateRelayReader;
        IRelayDevice relayDevice;
        RelayTestPointUpDown[] results;

        AutoResetEvent updateStateRelayAutoReset = new AutoResetEvent(false);

        public RelayTestTools(IPressSystem psys, int outChannelPsys, IStateRelayReader stateRelayReader, 
            IRelayDevice relayDevice)
        {
            this.psys = psys;
            this.outChannelPsys = outChannelPsys;
            this.stateRelayReader = stateRelayReader;
            this.relayDevice = relayDevice;
            stateRelayReader.StateReadEvent += (o, e) => updateStateRelayAutoReset.Set();
        }

        // public abstract int NumberOfRealays { get; }



        public void InitDevice()
        {
            relayDevice.Init();
        }

        protected void SendsettingsToDevice(double sp, double hysteresis)
        {
            relayDevice.WriteSettings(sp, hysteresis);
        }

        private void StartSmothSetPressure(double sp, double rangeMin, double rangeMax, bool absolutePressure, double rate,
            CancellationToken cancellationToken)
        {

            if (!psys.ConnectState && ((int)sp != 0 || absolutePressure))
            {
                // Если уставка отличается от 0 или это датчик абсолютного давления,
                // и пневмосистема еще не подключена, подключаем пневмосистему
                psys.Connect(outChannelPsys, cancellationToken);
            }
            if (psys.ConnectState)
            {
                psys.WriteNewSP(sp, rangeMin, rangeMax, absolutePressure, rate, cancellationToken);
            }
        }

        // Установка давления на максимальной скорости
        private void SetPressure(double sp, double rangeMin, double rangeMax, bool absolutePressure, 
            CancellationToken cancellationToken)
        {
            if (!psys.ConnectState && ((int)sp != 0 || absolutePressure))
            {
                // Если уставка отличается от 0 или это датчик абсолютного давления,
                // и пневмосистема еще не подключена, подключаем пневмосистему
                psys.Connect(outChannelPsys, cancellationToken);
            }
            if (psys.ConnectState)
            {
                psys.SetPressure(sp, rangeMin, rangeMax, absolutePressure, cancellationToken);
            }                    
        }

        private async void GetUpperPointsAsync(RelayTestPointSettings setpoints, bool absolutePressure, 
            PressureUnitsEnum pressureUnits, double marginCoefficient, CancellationToken cancellationToken)
        {

            // Устанавливаем нижнюю уставку (потом давление будет плавно изменяться от нижней уставки к верхней)
            SetPressure(setpoints.PsysDownwardSP, setpoints.RangeMin, setpoints.RangeMax, absolutePressure, cancellationToken);
            CheckStateRelays(false, cancellationToken);
            // Записываем уставку с ограничением изменения по скорости
            StartSmothSetPressure(setpoints.PsysUpwardSP, setpoints.RangeMin, setpoints.RangeMax, absolutePressure, setpoints.PsysRate,
                cancellationToken);
            double?[] pressures = await GetSwitchPressureValuesAsync(true, false, absolutePressure, setpoints.PressureLimitUpward,
                cancellationToken);

            for (int i = 0; i < pressures.Length; i++)
            {
                results[i].AddPointUp(pressures[i]);
            }
        }

        private async void GetDownPointsAsync(RelayTestPointSettings setpoints, bool absolutePressure,
            PressureUnitsEnum pressureUnits, double marginCoefficient, CancellationToken cancellationToken)
        {
            // Устанавливаем верхнюю уставку (потом давление будет плавно изменяться от верхней уставки к нижней)
            SetPressure(setpoints.PsysUpwardSP, setpoints.RangeMin, setpoints.RangeMax, absolutePressure, cancellationToken);
            CheckStateRelays(true, cancellationToken);
            // Записываем уставку с ограничением изменения по скорости
            StartSmothSetPressure(setpoints.PsysDownwardSP, setpoints.RangeMin, setpoints.RangeMax, absolutePressure, setpoints.PsysRate,
                cancellationToken);
            double?[] pressures = await GetSwitchPressureValuesAsync(true, false, absolutePressure, setpoints.PressureLimitUpward,
                cancellationToken);

            for (int i = 0; i < pressures.Length; i++)
            {
                results[i].AddPointDown(pressures[i]);
            }


        }

        private bool[] GetStateRelay(CancellationToken cancellationToken)
        {
            updateStateRelayAutoReset.Reset();
            while (!updateStateRelayAutoReset.WaitOne(100))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            return stateRelayReader.StateRelay;
        }

        // Проверяет состояние всех реле на соответствие ожидаемому
        private void CheckStateRelays(bool expectedState, CancellationToken cancellationToken)
        {
            bool[] stateRelays = GetStateRelay(cancellationToken);
            for (int i = 0; i < stateRelays.Length; i++)
            {
                if (stateRelays[i] ^ expectedState)
                {
                    throw new NotExpectedStateRelay(i);
                }
            }
        }

        //
        private async Task<double?[]> GetSwitchPressureValuesAsync(bool targetState, bool topDown, bool absolutePressure,
            double pressureLimit, CancellationToken cancellationToken)
        {
            Task<double?>[] tasks = new Task<double?>[stateRelayReader.StateRelay.Length];
            for(int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => GetSwitchPressure(i, targetState, topDown, absolutePressure, pressureLimit,
                    cancellationToken));
            }
            return await Task.WhenAll(tasks);
        }

        private double? GetSwitchPressure(int relayNumber, bool targetState, bool topDown, bool absolutePressure, 
            double pressureLimit, CancellationToken cancellationToken)
        {
            double shift = 0; 
            while (!cancellationToken.IsCancellationRequested)
            {
                bool[] state = GetStateRelay(cancellationToken);
                if (absolutePressure)
                    shift = psys.PressSystemVariables.Barometr;
                double? pressure = psys.PressSystemVariables.Pressure - shift;
                if (!(state[relayNumber] ^ targetState))
                    return pressure;
                if ((!topDown) && pressure.Value > pressureLimit)
                    return null;
                if (topDown && pressure.Value < pressureLimit)
                    return null;
            }
            return null;
        }

        public RelayTestPointUpDown[] GetTestPoints(RelayTestPointSettings settings, bool absolutePressure,
            PressureUnitsEnum pressureUnits, double marginCoefficient, CancellationToken cancellationToken)
        {
            SendsettingsToDevice(settings.DeviceSP, settings.Hysteresis);

            results = new RelayTestPointUpDown[stateRelayReader.StateRelay.Length];
            for(int i = 0; i < results.Length; i++)
            {
                results[i] = new RelayTestPointUpDown(settings, pressureUnits, marginCoefficient);
            }
            
            GetUpperPointsAsync(settings, absolutePressure, pressureUnits, marginCoefficient, cancellationToken);
            GetDownPointsAsync(settings, absolutePressure, pressureUnits, marginCoefficient, cancellationToken);
            return results;
        }
    }

    public class NotExpectedStateRelay: Exception
    {
        public override string Message { get; }
        public int RelayNumber { get; }

        public NotExpectedStateRelay(int relayNumber)
        {
            Message = $"Состояние дискретного выхода №{relayNumber} изделия не соответствует ожидаемому. " +
                $"Проверьте подключение изделия к стенду";
            RelayNumber = relayNumber;
        }
    }
}
