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
    public sealed class TestProcessForDigitalPort : ITestProcess
    {
        readonly IPressSystem psys;
        readonly IDigitalPort digitalPort;
        const double marginCoefficient = 0.8;
        readonly double[] testPoints; // Точки диапазона в долях

        bool psysIsConnect;

        public int NumberOfMeasurements { get; set; } = 1;
        public int TimeoutBetwinMeasures { get; set; } = 1; // в секундах

        public TestResults TestResults { get; private set; }
        public event EventHandler UpdResultsEvent;

        public TestProcessForDigitalPort(IPressSystem psys, IDigitalPort digitalPort, double[] testPoints, int pause100)
        {
            this.psys = psys;
            this.digitalPort = digitalPort;
            this.testPoints = testPoints;
            this.pause100 = pause100;
        }

        readonly Action<CancellationToken> waitContinue;
        public TestProcessForDigitalPort(Action<CancellationToken> waitContinue, IPressSystem psys, IDigitalPort digitalPort,
            double[] testPoints, int pause100) : this(psys, digitalPort, testPoints, pause100)
        {
            this.waitContinue = waitContinue;
        }

        double progressValue = 0;
        double deltaProgress = 0;
        int outChannelPsys;
        bool absoluteType;
        double rangeMin_Pa;
        double rangeMax_Pa;
        double classPrecision;
        PressureUnitsEnum pressureUnits;
        int pause100 = 60;



        // Диапазон принимается всегда в Па, в результатах давление будет в переданных единицах измерения  
        public void RunProcess(double rangeMin_Pa, double rangeMax_Pa, PressureUnitsEnum pressureUnits, double classPrecision, int outChannelPsys,
            bool absoluteType, CancellationToken cancellationToken, IProgress<int> progress)
        {
            try
            {
                this.outChannelPsys = outChannelPsys;
                this.absoluteType = absoluteType;
                this.rangeMin_Pa = rangeMin_Pa;
                this.rangeMax_Pa = rangeMax_Pa;
                this.pressureUnits = pressureUnits;
                this.classPrecision = classPrecision;
                deltaProgress = 100 / (testPoints.Length * 2 + 2);

                DeviceInit();
                TestResults = new TestResults(rangeMin_Pa, rangeMax_Pa, pressureUnits, classPrecision);
                UpdResultsEvent?.Invoke(this, new EventArgs());
                psysIsConnect = false;
                // Тест при движении вверх
                AddMeasureResults(TestResults.MeasureResultsUpwards, testPoints, cancellationToken, progress);
                // Пауза 1 мин
                Pause(cancellationToken, progress).GetAwaiter().GetResult();

                // Получаем точки для теста сверху вниз
                double[] points = new double[testPoints.Length];
                testPoints.CopyTo(points, 0);
                Array.Reverse(points);

                // Тест при движении вниз
                AddMeasureResults(TestResults.MeasureResultsTopdown, points, cancellationToken, progress);

                TestResults.CalcVariations();

                progress.Report(100);

                digitalPort.Dispose();

                UpdResultsEvent?.Invoke(this, new EventArgs());
            }
            catch
            {
                digitalPort.Dispose();
                throw;
            }
            
        }

        private void AddMeasureResults (MeasureResults measureResults, double[] points, 
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            foreach(double point in points)
            {
                measureResults.Add(CreateCheckPoint(point, cancellationToken));
                UpdResultsEvent?.Invoke(this, new EventArgs());
                
                progressValue += deltaProgress;
                progress.Report((int)progressValue);
                waitContinue?.Invoke(cancellationToken);
            }
        }

        private DigitalCheckPoint CreateCheckPoint(double testPoint, CancellationToken cancellationToken)
        {
            SetPressure(testPoint, rangeMin_Pa, rangeMax_Pa, cancellationToken);
            Measures(out double referencePressure, out double pressure, cancellationToken);
            // Если уставка равна нулю, то тестируемое изделие связано с атмосферой. Давление считаем равным нулю, 
            // кроме случая с абсолютным давлением
            if (((int)((rangeMax_Pa - rangeMin_Pa) * testPoint - rangeMin_Pa) == 0) && ! absoluteType)
                pressure = 0;
            DigitalCheckPoint point = new DigitalCheckPoint((int)(testPoint * 100), rangeMin_Pa, rangeMax_Pa, referencePressure, pressure, classPrecision,
                pressureUnits, marginCoefficient);
            return point;
        }

        private void SetPressure(double testPoint, double rangeMin, double rangeMax, CancellationToken cancellationToken)
        {
            double _rangeMin = rangeMin;
            double _rangeMax = rangeMax;
            // Находим уставку по точке диапазона
            double SP = testPoint * (_rangeMax - _rangeMin) + _rangeMin;
            
            if (!psysIsConnect && ((int)SP != 0 || absoluteType))
            {
                // Если уставка отличается от 0 или это датчик абсолютного давления,
                // и пневмосистема еще не подключена, подключаем пневмосистему
                psys.Connect(outChannelPsys, cancellationToken);
                psysIsConnect = psys.ConnectState;
            }
            if (psysIsConnect)
            {
                psys.SetPressure(SP, _rangeMin, _rangeMax, absoluteType, cancellationToken);
            }
                
        }

        private void Measures(out double referencePressure, out double pressure, CancellationToken cancellationToken)
        {
            double[] referencePressureItems = new double[NumberOfMeasurements];
            double[] pressureItems = new double[NumberOfMeasurements];
            for (int i = 0; i < NumberOfMeasurements; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Если пневмосистема не подключена, все порты стенда связаны с атмосферой
                // При этом давление на поверяемом преобразователе давления будет равно 0
                referencePressureItems[i] = psysIsConnect ? psys.PressSystemVariables.Pressure : 0;
                if (absoluteType)
                    referencePressureItems[i] += psys.PressSystemVariables.Barometr;
                pressureItems[i] = GetDevicePressurePa();
                if (i < NumberOfMeasurements - 1)
                    Thread.Sleep(TimeoutBetwinMeasures*1000);
            }
            referencePressure = referencePressureItems.Sum() / referencePressureItems.Length;
            pressure = Math.Round(pressureItems.Sum() / referencePressureItems.Length, 4, MidpointRounding.AwayFromZero);
        }

        private void DeviceInit()
        {
            try
            {
                digitalPort.Init();
            }
            catch (Exception ex)
            {
                throw new ConnectException();
            }
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

        private async Task Pause(CancellationToken cancellation, IProgress<int> progress)
        {
            bool exit = false;
            var startTime = DateTime.Now;
            double startProgerss = progressValue;
            using (System.Timers.Timer timer = new System.Timers.Timer(pause100 * 1000))
            {
                timer.Elapsed += (obj, e) => exit = true;
                timer.Start();
                while(!exit)
                {
                    var timeSpan = DateTime.Now - startTime;
                    progressValue = startProgerss + (2*deltaProgress * timeSpan.Seconds / pause100);
                    progress.Report((int)progressValue);
                    cancellation.ThrowIfCancellationRequested();
                    await Task.Delay(500);
                }
            }   
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
    }
    [Serializable]
    public class OpenConnectPortException: Exception { }
    [Serializable]
    public class ConnectException: Exception { }
    [Serializable]
    public class LostConnectException: Exception { }
}
