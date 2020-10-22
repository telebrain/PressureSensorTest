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
    public class TestProcess : ITestProcess
    {
        readonly IPressSystem psys;
        readonly IAmmetr ammetr;
        const double marginCoefficient = 0.8;
        readonly double[] testPoints; // Точки диапазона в долях

        bool psysIsConnect;

        public int NumberOfMeasurements { get; set; } = 1;
        public int TimeoutBetwinMeasures { get; set; } = 1; // в секундах

        public TestResults TestResults { get; private set; }
        public event EventHandler UpdResultsEvent;


        public TestProcess(IPressSystem psys, IAmmetr ammetr, double[] testPoints)
        {
            this.psys = psys;
            this.ammetr = ammetr;
            this.testPoints = testPoints;
        }

        readonly Action<CancellationToken> waitContinue;

        public TestProcess(Action<CancellationToken> waitContinue, IPressSystem psys, IAmmetr ammetr, 
            double[] testPoints): this(psys, ammetr, testPoints)
        {
            this.waitContinue = waitContinue;
        }

        double progressValue = 0;
        double deltaProgress = 0;
        AutoResetEvent updCurrentValueAutoReset;
        int outChannelPsys;
        bool absoluteType;
        

        public void RunProcess(double rangeMin, double rangeMax, double classPrecision, int outChannelPsys,
            bool absoluteType, CancellationToken cancellationToken, IProgress<int> progress)
        {
            this.outChannelPsys = outChannelPsys;
            this.absoluteType = absoluteType;
            updCurrentValueAutoReset = new AutoResetEvent(false);
            // При обновлении показаний миллиамперметра будет освобождаться AutoResetEvent updCurrentValueAutoReset
            ammetr.UpdMeasureResult += (obj, e) => updCurrentValueAutoReset.Set(); 
            deltaProgress = 100/(testPoints.Length*2 + 2);


            TestResults = new TestResults(rangeMin, rangeMax, classPrecision);
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
            UpdResultsEvent?.Invoke(this, new EventArgs());

        }

        private void AddMeasureResults (MeasureResults measureResults, double[] points, 
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            foreach(double point in points)
            {
                measureResults.Add(CheckPointMeasurmentsProcess(measureResults.RangeMin, measureResults.RangeMax, point, 
                    measureResults.ClassPrecision, cancellationToken));
                UpdResultsEvent?.Invoke(this, new EventArgs());
                
                progressValue += deltaProgress;
                progress.Report((int)progressValue);
                waitContinue?.Invoke(cancellationToken);
            }
        }

        private CheckPoint CheckPointMeasurmentsProcess(double rangeMin, double rangeMax, double testPoint, double classPrecision, 
            CancellationToken cancellationToken)
        {
            SetPressure(testPoint, rangeMin, rangeMax, cancellationToken);
            Measures(out double pressure, out double current, cancellationToken);
            // Если уставка равна нулю, то тестируемое изделие связано с атмосферой. Давление считаем равным нулю
            if ((int)((rangeMax - rangeMin) * testPoint - rangeMin) == 0)
                pressure = 0;
            CheckPoint point = new CheckPoint((int)(testPoint * 100), pressure, current);
            return point;
        }

        private void SetPressure(double testPoint, double rangeMin, double rangeMax, CancellationToken cancellationToken)
        {
            double _rangeMin = rangeMin;
            double _rangeMax = rangeMax;
            // Находим уставку по точке диапазона
            double SP = testPoint * (_rangeMax - _rangeMin) + _rangeMin;
            if (absoluteType)
            {
                // Если поверка прибора абсолютного давления, корректируем уставку и диапазон по барометру
                SP -= psys.PressSystemVariables.Barometr;
                _rangeMin -= psys.PressSystemVariables.Barometr;
                _rangeMax -= psys.PressSystemVariables.Barometr;
            }
            if (!psysIsConnect && (int)SP != 0)
            { 
                // Если пневмосистема еще не была подключена, а уставка отличается от 0, подключаем пневмосистему
                psys.Connect(outChannelPsys, cancellationToken);
                psysIsConnect = psys.ConnectState;
            }
            if (psysIsConnect)
                psys.SetPressure(SP, _rangeMin, _rangeMax, cancellationToken);
        }

        private void Measures(out double pressure, out double current, CancellationToken cancellationToken)
        {
            double[] pressureItems = new double[NumberOfMeasurements];
            double[] currentItems = new double[NumberOfMeasurements];
            for (int i = 0; i < NumberOfMeasurements; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Если пневмосистема не подключена, все порты стенда связаны с атмосферой
                // При этом давление на поверяемом преобразователе давления будет равно 0
                pressureItems[i] = psysIsConnect ? psys.PressSystemVariables.Pressure : 0;
                if (absoluteType)
                    pressureItems[i] += psys.PressSystemVariables.Barometr;
                currentItems[i] = GetCurrent(cancellationToken);
                CheckCurrent(currentItems[i]);
                if (i < NumberOfMeasurements - 1)
                    Thread.Sleep(TimeoutBetwinMeasures*1000);
            }
            pressure = pressureItems.Sum() / pressureItems.Length;
            current = Math.Round(currentItems.Sum() / pressureItems.Length, 4);
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
            while (ammetr.Timestamp <= pressureTimeStamp);
            return ammetr.Current;
        }

        private async Task Pause(CancellationToken cancellation, IProgress<int> progress)
        {
            const int pause = 60; // Заменить потом на 60 с
            bool exit = false;
            var startTime = DateTime.Now;
            double startProgerss = progressValue;
            using (System.Timers.Timer timer = new System.Timers.Timer(pause * 1000))
            {
                timer.Elapsed += (obj, e) => exit = true;
                timer.Start();
                while(!exit)
                {
                    var timeSpan = DateTime.Now - startTime;
                    progressValue = startProgerss + (2*deltaProgress * timeSpan.Seconds / pause);
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

    public class LoCurrentAlarmException : Exception { }
    public class HiCurrentAlarmException : Exception { }
}
