using System;
using System.Threading;
using System.Threading.Tasks;
using PressSystems;
using SDM_comm;
using OwenPressureDevices;
using PressureSensorTestCore;
using MetrologicUtils;
using PressureRack;




namespace PressureSensorTest
{
    public class Stand
    {
        IPressSystem psys;
        IAmmetr ammetr;

        #region События

        public event EventHandler UpdRowsTypesEvent;
        public event EventHandler UpdProductInfoEvent;
        public event EventHandler UpdTestResultEvent;
        public event EventHandler ContinueRequest;
        public event EventHandler SelectionRequest;
        public event EventHandler<int> ProgressEvent;
        public event EventHandler UpdateMeasurmendIndicators;
        public event EventHandler ProcessComplete; // Вызывается при удачном (не аварийном) завершении процесса
        public event EventHandler RemoteStartEvent; 
        public event EventHandler StopEvent; // Вызывается при любом завершении процесса

        #endregion


        public IDeviceSpecification DeviceSpecification { get; private set; }
        public ProductInfo Product{ get; private set; }
        public TestResults TestResults { get; private set; }
        public Exception Exception { get; private set; }
        public PressSystemInfo PressSystemInfo { get; private set; }
        public SystemStatus SystemStatus { get; private set; }
        
        public Settings Settings { get; }

        // IRemoteControl remoteControl;

        public Stand()
        {
            Settings = new Settings();
            Settings.Load();
            progress = new Progress();
            progress.ProgressChanged += (obj, val) => ProgressEvent(this, val);
            SystemStatus = new SystemStatus();
        }

        RemoteControl remoteControl;

        public void Init(IDialogService dialogService)
        {
            try
            {
                this.dialogService = dialogService;
                Exception = null;
                // var psysCommands = new PsysCommandSimulator();
                var psysCommands = new Commands(Settings.PsysSettings.IP, 49002);
                psys = new PressSystem(psysCommands, 20);
                SystemStatus.Init(Settings);
                psys.ExceptionEvent += Exception_psys_event;
                psys.ConnectEvent += SystemStatus.PressSysten_ConnectEvent;
                psys.DisconnectEvent += SystemStatus.PressSystemDisconnectEvent;
                psys.BeginConnectEvent += SystemStatus.PressSystem_BeginConnectEvent;
                // InitAmmetr();
                
                savingResults = new SavingResults(Settings, SystemStatus);
                remoteControl?.Dispose();

                if (!Settings.UsedRemoteControl)
                {
                    processErrorHandler = new ErrorHandler(Settings, SystemStatus, dialogService);
                    ReadPsysInfo();
                }
                else
                {
                    processErrorHandler = new ErrorHandlerRemoteControlMode(Settings, SystemStatus);
                    remoteControl = new RemoteControl(this, Settings.RemoteControlIp, 49003);
                    remoteControl.StartListening();
                }
                // throw new Exception();
                
            }
            catch (PressSystemException ex)
            {
                Exception = ex;
                dialogService.ErrorMessage("Не удалось установить связь со стойкой давления по запросу. Проверьте состояние ее готовности  " +
                                        "и нажмите кнопку \"Установить связь со стойкой давления\". Или измените настройки в меню \"Система\"");
            }
            catch (Exception ex)
            {
                Exception = ex;
                dialogService.ErrorMessage(ex.Message);
            }
        }

        CancellationTokenSource cts;
        MeasurmendIndicator measurmendIndicator;
        WaitContinue waitContinue;
        Progress progress;
        ErrorHandler processErrorHandler; // Обработчик ошибок процесса тестирования (только их!)
        SavingResults savingResults;

        IDialogService dialogService;

        // Старт процесса в режиме ручного управления
        public void Start(string serialNumber, string deviceName, CancellationTokenSource cts)
        {
            try
            {
                CheckSerialNumber(serialNumber);
                this.cts = cts;
                IDevice device = new PD100_Device(serialNumber, deviceName);
                Product = new ProductInfo(device, DateTime.Now);
                Start(device, true, cts.Token);
                savingResults.SaveResult(Product, TestResults, dialogService);
                ProcessComplete?.Invoke(this, new EventArgs());
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                dialogService.ErrorMessage(ex.Message);
            }
          
        }

        // Старт процесса в режиме удаленного управления
        public async Task RemoteStart(IDevice device, string box, DateTime dateTime, bool primaryVerification)
        {
            try
            {
                Product = new ProductInfo(device, box, dateTime, primaryVerification);
                RemoteStartEvent?.Invoke(this, new EventArgs());
                cts = new CancellationTokenSource();
                await Task.Run(() => Start(device, primaryVerification, cts.Token));
                savingResults.SaveResult(Product, TestResults, dialogService);
                StopEvent?.Invoke(this, new EventArgs());
                ProcessComplete?.Invoke(this, new EventArgs());
            }
            catch (OperationCanceledException)
            {
                StopEvent?.Invoke(this, new EventArgs());
            }        
        }

        public void RemoteCancel()
        {
            cts?.Cancel();
        }

        private void Start(IDevice device, bool primaryVerification, CancellationToken cancellation)
        {
            try
            {
                Exception = null;
                InitAmmetr(); 
                ReadPsysInfo();
                DeviceSpecification.CheckRangeSupport(device);
                UpdProductInfoEvent?.Invoke(this, new EventArgs());               
                ammetr.StartCycleMeasureCurrent();
                measurmendIndicator = new MeasurmendIndicator(ammetr, psys, device.Range.RangeType == RangeTypeEnum.DA);
                measurmendIndicator.UpdDataEvent += UpdateMeasurmendIndicators;
                measurmendIndicator.Start();

                TestProcess testProcess;
                if (Settings.UsedAutomaticSortingOut)
                {
                    waitContinue = null;
                    testProcess = new TestProcess(psys, ammetr, GetTestPoints(device.Range.Max, device.Range.Min));
                }
                else
                {
                    waitContinue = new WaitContinue();
                    waitContinue.ContinueRequest += ContinueRequest;
                    waitContinue.SelectionRequest += (obj, e) => SelectionRequest(this, e);
                    testProcess = new TestProcess(waitContinue.Wait, psys, ammetr, GetTestPoints(device.Range.Max, device.Range.Min));
                }
                testProcess.UpdResultsEvent += UpdTestResult_event;
                progress.Report(0);
                double precision = primaryVerification ? device.TargetPrecision : device.ClassPrecision;
                testProcess.RunProcess(device.Range.Min, device.Range.Max, precision, GetPsysOutChannel(),
                        device.Range.RangeType == RangeTypeEnum.DA, cancellation, progress);
                if (TestResults.GetResume() != true)
                    Product.Error = TestErrorEnum.BadPrecision;
                else
                    Product.Error = TestErrorEnum.NoError;
                Stop();
                waitContinue?.WaitSelection(cancellation);
            }
            catch (OperationCanceledException)
            {
                Product.Error = TestErrorEnum.InDefined;
                Stop();
                if (Exception != null)
                    processErrorHandler.ErrorHanding(Exception, Product);
                else
                    throw;
            }
            catch(Exception ex)
            {
                Product.Error = TestErrorEnum.InDefined;
                Stop();
                Exception = ex;
                processErrorHandler.ErrorHanding(ex, Product);
            }
        }

        // В режиме ручной разбраковки будет обрабатываться нажмтие кнопки "Далее"
        public void NextStep()
        {
            if (waitContinue != null)
                waitContinue.Continue();
        }

        public void Release()
        {
            Product.Error = TestErrorEnum.NoError;
            waitContinue.Continue();
        }

        public void Reject()
        {
            if (Product.Error == TestErrorEnum.NoError)
                Product.Error = TestErrorEnum.OperatorSolution;
            waitContinue.Continue();
        }

        public void ShowAboutTheProgramm()
        {
            var metrologicInfo = new MetrologicInfo();
            string info = $"Версия сборки v.{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}" +
                $"\n{new string('-', 80)}\n\n" + metrologicInfo.GetMetrologicInfo();
            
            dialogService.Message("О программе", info);
        }

        private void Stop()
        {
            if (psys != null)
                psys.Disconnect();
            if (ammetr != null)
                ammetr.Stop();
            if (measurmendIndicator != null)
                measurmendIndicator.Stop();
        }

        private void Exception_psys_event(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Отмена из второго потока");
            Exception = ((IPressSystem)sender).Exception;
            cts.Cancel();
        }

        private void Exception_ammetr_event(object sender, EventArgs e)
        {
            Exception = ((IAmmetr)sender).Exception;
            cts.Cancel();
        }

        private void UpdTestResult_event(object sender, EventArgs e)
        {
            TestResults = ((TestProcess)sender).TestResults;
            UpdTestResultEvent?.Invoke(this, new EventArgs());
        }

        private void ReadPsysInfo()
        {
            psys.ReadInfo();
            PressSystemInfo = psys.Info;
            DeviceSpecification = new PD100_DeviceSpecification(PressSystemInfo);
            UpdRowsTypesEvent?.Invoke(this, new EventArgs());
        }

        private double[] GetTestPoints(double rangeMax, double rangeMin)
        {
            double[] targetPoints =  new double[] { 0, 0.25, 0.5, 0.75, 1 };
            double span = rangeMax - rangeMin;
            for (int i = 0; i < targetPoints.Length; i++)
            {
                double pressPoint = targetPoints[i] * span + rangeMin;
                if (pressPoint < psys.Info.RangeLo)
                {
                    pressPoint = psys.Info.RangeLo;
                    targetPoints[i] = (pressPoint - rangeMin) / span;
                }
                else if (pressPoint > psys.Info.RangeHi)
                {
                    pressPoint = psys.Info.RangeHi;
                    targetPoints[i] = (pressPoint - rangeMin) / span;
                }
            }
            return targetPoints;
        }

        private int GetPsysOutChannel()
        {
            if (Product.Device.ThreadType == "7")
                return Settings.PsysSettings.ChannelsOut[1];
            if (Product.Device.ThreadType == "8")
                return Settings.PsysSettings.ChannelsOut[2];
            return Settings.PsysSettings.ChannelsOut[0];

        }

        private void InitAmmetr()
        {
            ammetr = new Ammetr(Settings.AmmetrSettins.Ip, CurrentTypeEnum.DC, CurrentUnitsEnum.AUTO, 20);

            // Для симуляции
            //ammetr = new AmmetrSimulator(psys, Product.Device.Range.Min, Product.Device.Range.Max, 0.05,
            //    Product.Device.Range.RangeType == RangeTypeEnum.DA);

            ammetr.ExceptionEvent += Exception_ammetr_event;
            ammetr.ConnectEvent += SystemStatus.Ammetr_ConnectEvent;
        }

        private void CheckSerialNumber(string serialNumber)
        {
            // Если не используется удаленное управление, но программа работает с базами данных, нужно проверить, введен ли серийный номер
            if (!Settings.UsedRemoteControl && (Settings.UsedStandDatabase || Settings.JsonReportSettings.ArchivingJsonFile))
            {
                if (string.IsNullOrEmpty(serialNumber))
                    throw new Exception("Не введен заводской номер изделия");
            }
        }
    }
}
