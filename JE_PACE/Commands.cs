using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressSystems;

namespace JE_PACE
{
    public class Commands : IPressSystemCommands, IDisposable
    {

        Pace pace = null;
        bool pace6000 = false;
        double precision = 0.02;
        const int VacoomPressure = -100000;
        const int operationAttempts = 3;
        IExchange exchange = null;
        double srcPlus;
        double srcMinus;
        int modulWithBarometer = -1;
        const double souceMargin = 0.05;
        const int thresholdUnitPA = 100000;

        PressSystemInfo paceConfig = null;

        int currentChannel = -1; // -1 - не определен


        public Commands(string portName, double srcPlus, double srcMinus, bool pace6000 = false, double precision = 0.02) :
            this(srcPlus, srcMinus, pace6000, precision)
        {
            exchange = new ComExchange(portName);
            pace = new Pace(exchange);
        }

        public Commands(string ip, int portNumber, double srcPlus, double srcMinus, bool pace6000 = false, double precision = 0.02)
        {
            throw new NotImplementedException();
        }

        Commands(double srcPlus, double srcMinus, bool pace6000, double precision)
        {
            this.pace6000 = pace6000;
            this.srcPlus = srcPlus;
            this.srcMinus = srcMinus;
            this.precision = precision;
        }

        public void Connect(int outChannelNumber, CancellationToken cancellationToken)
        {
            if (paceConfig == null)
                paceConfig = ReadInfo();

            for (int i = 0; i < 2; i++)
            {
                if (paceConfig.Controllers[i].RangeHi < thresholdUnitPA)
                    WriteUnit(operationAttempts, PaceUnitsEnum.PA, i);
                else
                    WriteUnit(operationAttempts, PaceUnitsEnum.KPA, i);
            }

            if ((!pace6000) || paceConfig.Controllers[0].RangeHi < paceConfig.Controllers[1].RangeHi)
                currentChannel = 0;
            else
                currentChannel = 1;
            

            pace.Connect();
        }

        private void WriteUnit(int attempts, PaceUnitsEnum unit, int channel)
        {
            try
            {
                pace.WriteUnitWithCheck(unit, channel);
            }
            catch
            {
                attempts -= 1;
                if (attempts <= 0)
                    throw;
                WriteUnit(attempts, unit, channel);
            }
        }

        public void DisableControl()
        {
            pace.WriteOutStateWithCheck(false);
        }

        public void Disconnect()
        {
            try
            {
                pace.WriteSpWithCheck(0);
                if (pace6000)
                    pace.WriteSpWithCheck(1);
                pace.Dispose();
            }
            catch { }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public PressSystemInfo ReadInfo()
        {
            PressControllersList modules = new PressControllersList();
            AddModul(modules);
            if (pace6000)
                AddModul(modules, 1);
            paceConfig = new PressSystemInfo();
            paceConfig.RangeHi = srcPlus;
            paceConfig.RangeLo = srcMinus;
            paceConfig.Controllers = modules;
            return paceConfig;
        }

        private void AddModul(PressControllersList modules, int number = 0)
        {
            PressControllerInfo modul = new PressControllerInfo();
            modul.IsEnabled = true;
            modul.Number = number + 1;
            modul.Precision = precision;
            modul.SN = ReadSN(operationAttempts, number);
            modul.RangeHi = ReadRange(operationAttempts, number);
            if (modul.RangeHi > Math.Abs(VacoomPressure))
                modul.RangeLo = VacoomPressure;
            else
                modul.RangeLo = (-1) * modul.RangeHi;
            if (pace.CheckBarometer())
                modulWithBarometer = number;
            modules.Add(modul);
        }

        private string ReadSN(int operationAttempts, int channel = 0)
        {
            try
            {
                string val = pace.GetModulSn(channel);
                return val;
            }
            catch
            {
                operationAttempts -= 1;
                if (operationAttempts <= 0)
                    throw;
                return pace.GetModulSn(channel);
            }
        }

        private double ReadRange(int operationAttempts, int channel = 0)
        {
            try
            {
                double val = pace.GetRange(out double loLim, out double hiLimit);
                return val;
            }
            catch
            {
                operationAttempts -= 1;
                if (operationAttempts <= 0)
                    throw;
                return ReadRange(operationAttempts, channel);
            }
        }

        public PressSystemVariables ReadSysVar()
        {
            ReadVar(operationAttempts, out double pressure, out double barPressure, out bool inLim, currentChannel);
            return new PressSystemVariables()
            {
                Pressure = pressure,
                Barometr = barPressure,
                InLim = inLim,
                TimeStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
            };
        }

        private void ReadVar(int attempts, out double pressure, out double barPressure, out bool inLim, int channel)
        {
            try
            {
                pressure = pace.ReadPressure(channel);
                if (modulWithBarometer >= 0)
                    barPressure = pace.ReadBarPressure(modulWithBarometer);
                else
                    barPressure = 0;
                inLim = pace.GetInLim(channel);
            }
            catch
            {
                attempts -= 1;
                if (attempts <= 0)
                    throw;
                ReadVar(attempts, out pressure, out barPressure, out inLim, channel);
            }
        }

        public void WriteSP(int controller, double SP, CancellationToken cancellationToken)
        {
            currentChannel = controller - 1;
            CheckSrc((int)SP, currentChannel);
            pace.WriteSpWithCheck((int)SP, currentChannel);
            pace.WriteOutStateWithCheck(true, currentChannel);
        }

        private void CheckSrc(int sp, int channel = 0)
        {
            if (sp == 0)
                return;
            if (sp > (1 - souceMargin) * ReadDoubleParametr(operationAttempts, pace.ReadSourcePlus, channel))
                throw new AlarmSourcePlusException();
            else if (sp < (1 - souceMargin) * ReadDoubleParametr(operationAttempts, pace.ReadSourceMinus, channel))
                throw new AlarmSourceMinusException();
        }

        public double ReadDoubleParametr(int attempts, Func<int, double> func, int channel = 0)
        {
            try
            {
                double val = func(channel);
                return val;
            }
            catch
            {
                attempts -= 1;
                if (attempts <= 0)
                    throw;
                return ReadDoubleParametr(attempts, func, channel);
            }
        }
    }

    public class AlarmSourcePlusException : Exception
    {
        public AlarmSourcePlusException(): base("Для данной устаки давление источника \"+\" слишком мало") { }
    }

    public class AlarmSourceMinusException : Exception
    {
        public AlarmSourceMinusException() : base("Для данной устаки давление источника \"-\" слишком велико") { }
    }
}
