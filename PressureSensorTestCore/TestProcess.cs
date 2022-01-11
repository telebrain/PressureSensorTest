using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PressSystems;
using SDM_comm;


namespace PressureSensorTestCore
{
    public class TestProcess: ITestProcess, IDisposable
    {
        const double marginCoefficient = 0.8;
        readonly double[] testPoints; // Точки измерения в долях диапазона
        readonly double[] relayTestPoints; // Точки тестирования реде в долях диапазона

        readonly IMeasurmentTools measurmentTools;
        readonly IRelayTestTools relayTestTools;


        public int NumberOfMeasurements { get; set; } = 1;
        public int TimeoutBetwinMeasures { get; set; } = 1; // в секундах

        public TestResults TestResults { get; private set; }

        public RelayTestResults[] RelayTestResults { get; private set; }

        public event EventHandler UpdResultsEvent;

        

        public TestProcess(IMeasurmentTools measurmentTools, double[] testPoints, int pause100)
        {
            this.testPoints = testPoints;
            this.pause100 = pause100;
            this.measurmentTools = measurmentTools;
            this.relayTestTools = null;
            this.relayTestPoints = null;
            deltaProgress = 100 / (testPoints.Length * 2 + 2);
        }

        public TestProcess(IMeasurmentTools measurmentTools, double[] testPoints, int pause100, IRelayTestTools relayTestTools, 
            double[] relayTestPoints)
        {
            this.testPoints = testPoints;
            this.pause100 = pause100;
            this.measurmentTools = measurmentTools;
            this.relayTestTools = relayTestTools;
            this.relayTestPoints = relayTestPoints;
            deltaProgress = 100 / (relayTestPoints.Length + testPoints.Length * 2 + 2);
        }


        readonly Action<CancellationToken> waitContinue;
        public TestProcess(Action<CancellationToken> waitContinue,  IMeasurmentTools measurmentTools, 
            double[] testPoints, int pause100): this(measurmentTools, testPoints, pause100)
        {
            this.waitContinue = waitContinue;
        }

        double progressValue = 0;
        double deltaProgress = 0;
        bool absoluteType = false;
        double rangeMin_Pa;
        double rangeMax_Pa;
        double classPrecision;
        PressureUnitsEnum pressureUnits;
        int pause100 = 60;

        List<ProgramStep> program;

        class ProgramStep
        {
            internal Action Action { get; }
            internal ProgramStep(Action action)
            {
                Action = action;
            }
        }

        class PressPointAction : ProgramStep
        {
            internal double PressurePoint { get; }
            internal PressPointAction(double pressurePoint, Action action): base(action)
            {
                PressurePoint = pressurePoint;
            }
        }

        private void CreateProgram(CancellationToken cancellationToken, IProgress<int> progress)
        {
            program = new List<ProgramStep>();
            TestResults = new TestResults(rangeMin_Pa, rangeMax_Pa, pressureUnits, classPrecision);
            // Шаги для теста при движении вверх
            foreach (var point in testPoints)
            {
                program.Add(new PressPointAction(point, new Action(() => 
                    AddMeasureTestPoint(point, TestResults.MeasureResultsUpwards, cancellationToken, progress))));
            }

            if (relayTestPoints != null)
            {
                foreach(var point in relayTestPoints)
                {
                    for(int i = 0; i < program.Count; i++)
                    {
                        if(program[i] is PressPointAction && point > ((PressPointAction)program[i]).PressurePoint)
                        {
                            program.Insert(i, new PressPointAction(point, new Action(() =>
                                AddRelayTestPoint(point, cancellationToken, progress))));
                        }
                    }
                }
            }

            // Пауза 1 мин
            program.Add(new ProgramStep(new Action(() => Pause(cancellationToken, progress).GetAwaiter().GetResult())));

            // Получаем точки для теста сверху вниз
            double[] points = new double[testPoints.Length];
            testPoints.CopyTo(points, 0);
            Array.Reverse(points);

            // Шаги теста при движении вниз
            foreach (var point in points)
            {
                program.Add(new PressPointAction(point, new Action(() =>
                    AddMeasureTestPoint(point, TestResults.MeasureResultsTopdown, cancellationToken, progress))));
            }

            // Вычисление вариации
            program.Add(new ProgramStep(new Action(() => TestResults.CalcVariations())));
        }

        // Диапазон принимается всегда в Па, в результатах давление будет в переданных единицах измерения  
        public void RunProcess(double rangeMin_Pa, double rangeMax_Pa, PressureUnitsEnum pressureUnits, double classPrecision, 
            bool absoluteType, CancellationToken cancellationToken, IProgress<int> progress)
        {
            this.absoluteType = absoluteType;
            this.rangeMin_Pa = rangeMin_Pa;
            this.rangeMax_Pa = rangeMax_Pa;
            this.pressureUnits = pressureUnits;
            this.classPrecision = classPrecision;
            

            measurmentTools.Init();

            CreateProgram(cancellationToken, progress);
            UpdResultsEvent?.Invoke(this, new EventArgs());

            foreach (var step in program)
            {
                step.Action?.Invoke();
                
            }


            progress.Report(100);
            UpdResultsEvent?.Invoke(this, new EventArgs());

            measurmentTools.Dispose();
            
        }

        private void AddMeasureTestPoint(double point, MeasureResults measureResults, 
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            ICheckPoint checkPoint = measurmentTools.GetCheckPoint(point, rangeMin_Pa, rangeMax_Pa, absoluteType, pressureUnits,
                    classPrecision, marginCoefficient, cancellationToken);
            if (measureResults == null)
                measureResults = new MeasureResults();
            measureResults.Add(checkPoint);
            UpdResultsEvent?.Invoke(this, new EventArgs());
            progressValue += deltaProgress;
            progress.Report((int)progressValue);
            waitContinue?.Invoke(cancellationToken);
        }

        private void AddRelayTestPoint(double point, CancellationToken cancellationToken, IProgress<int> progress)
        {
            var settings = new RelayTestPointSettings(point, rangeMin_Pa, rangeMax_Pa, classPrecision);
            RelayTestPointUpDown[] relaysTestPoint = relayTestTools.GetTestPoints(settings, absoluteType, pressureUnits,
                marginCoefficient, cancellationToken);
            if (RelayTestResults == null)
                RelayTestResults = new RelayTestResults[relaysTestPoint.Length];
            for (int i = 0; i < relaysTestPoint.Length; i++)
            {
                if (RelayTestResults[i] == null)
                    RelayTestResults[i] = new RelayTestResults();
                RelayTestResults[i].Add(relaysTestPoint[i]);
            }
            UpdResultsEvent?.Invoke(this, new EventArgs());
            progressValue += deltaProgress;
            progress.Report((int)progressValue);
            waitContinue?.Invoke(cancellationToken);
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
                while (!exit)
                {
                    var timeSpan = DateTime.Now - startTime;
                    progressValue = startProgerss + (2 * deltaProgress * timeSpan.Seconds / pause100);
                    progress.Report((int)progressValue);
                    cancellation.ThrowIfCancellationRequested();
                    await Task.Delay(500);
                }
            }
        }


        public void Dispose()
        {
            
        }
    }


}
