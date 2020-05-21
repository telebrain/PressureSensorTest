using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PressSystems;
using SDM_comm;


namespace PressureSensorTestCore
{
    public class TestProcess : ITestProcess
    {
        readonly IPressSystem psys;
        readonly IAmmetr ammetr;
        readonly double marginCoefficient;
        readonly double[] testPoints;

        bool psysIsConnect;

        public int NumberOfMeasurements { get; set; } = 1;
        public int TimeoutBetwinMeasures { get; set; } = 1; // в секундах

        public TestResults TestResults { get; private set; }
        public event EventHandler UpdResultsEvent;


        public TestProcess(IPressSystem psys, IAmmetr ammetr, double[] testPoints, 
            double marginCoefficient = 0.8)
        {
            this.psys = psys;
            this.ammetr = ammetr;
            this.marginCoefficient = marginCoefficient;
            this.testPoints = testPoints;
        }

        readonly Action<CancellationToken> waitContinue;

        public TestProcess(Action<CancellationToken> waitContinue, IPressSystem psys, IAmmetr ammetr, 
            double[] testPoints, double marginCoefficient = 0.8): 
            this(psys, ammetr, testPoints, marginCoefficient)
        {
            this.waitContinue = waitContinue;
        }

        double progressValue = 0;
        double deltaProgress = 0;

        public void RunProcess(double rangeMin, double rangeMax, double classPrecision, int outChannelPsys,
            CancellationToken cancellation, IProgress<int> progress)
        {
            deltaProgress = 100/(testPoints.Length*2);
            TestResults = new TestResults(rangeMin, rangeMax, classPrecision);
            UpdResultsEvent?.Invoke(this, new EventArgs());
            psysIsConnect = false;
            // Тест при движении вверх
            AddMeasureResults(TestResults.MeasureResultsUpwards, outChannelPsys, testPoints, cancellation, progress);
            // Пауза 1 мин
            Pause(cancellation);

            // Получаем точки для теста сверху вниз
            double[] points = new double[testPoints.Length];
            testPoints.CopyTo(points, 0);
            Array.Reverse(points);
            
            // Тест при движении вниз
            AddMeasureResults(TestResults.MeasureResultsTopdown, outChannelPsys, points, cancellation, progress);

            TestResults.CalcVariations();

            progress.Report(100);
            UpdResultsEvent?.Invoke(this, new EventArgs());

        }

        private void AddMeasureResults (MeasureResults measureResults, int outChannelPsys, double[] points, CancellationToken cancellation, 
            IProgress<int> progress)
        {
            foreach(double point in points)
            {
                measureResults.Add(CheckPointMeasurmentsProcess(measureResults.RangeMin, measureResults.RangeMax, point, 
                    measureResults.ClassPrecision, outChannelPsys, cancellation));
                UpdResultsEvent?.Invoke(this, new EventArgs());
                
                progressValue += deltaProgress;
                progress.Report((int)progressValue);
                waitContinue?.Invoke(cancellation);
            }
        }

        private CheckPoint CheckPointMeasurmentsProcess(double rangeMin, double rangeMax, double testPoint, double classPrecision, int outChannelPsys, 
            CancellationToken cancellation)
        {
            double SP = GetPressureByPoint(rangeMin, rangeMax, testPoint);
            if (!psysIsConnect && (int)SP != 0)
            {
                // Если пневмосистема еще не была подключена, а устанвка отличается от 0, подключаем пневмосистему
                psys.Connect(outChannelPsys, cancellation);
                psysIsConnect = psys.ConnectState;
            }
            if (psysIsConnect)
                psys.SetPressure(SP, rangeMin, rangeMax, cancellation);
            Thread.Sleep(1000);
            Measures(out double pressure, out double current, cancellation);
            CheckPoint point = new CheckPoint((int)(testPoint * 100), pressure, current);
            return point;
        }

        private void Measures(out double pressure, out double current, CancellationToken cancellation)
        {
            double[] pressureItems = new double[NumberOfMeasurements];
            double[] currentItems = new double[NumberOfMeasurements];
            for (int i = 0; i < NumberOfMeasurements; i++)
            {
                cancellation.ThrowIfCancellationRequested();
                // Если пневмосистема не подключена, все порты стенда связаны с атмосферой
                // При этом давление на поверяемом преобразователе давления будет равно 0
                pressureItems[i] = psysIsConnect ? psys.PressSystemVariables.Pressure : 0;
                currentItems[i] = ammetr.Current;
                if (i < NumberOfMeasurements - 1)
                    Thread.Sleep(TimeoutBetwinMeasures*1000);
            }
            pressure = pressureItems.Sum() / pressureItems.Length;
            current = currentItems.Sum() / pressureItems.Length;
        }

        private double GetPressureByPoint(double RangeMin, double RangeMax, double testPoint)
        {
            return testPoint*(RangeMax - RangeMin) + RangeMin;
        }

        private void Pause(CancellationToken cancellation)
        {
            Thread.Sleep(1000); // Это пока
        }
        
    }
}
