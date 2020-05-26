using System;
using System.Threading;
using PressSystems;
using SDM_comm;
using OwenPressureDevices;
using PressureSensorTestCore;
using MetrologicUtils;



namespace PressureSensorTest
{
    public class Stand
    {
        IPressSystem psys;
        IAmmetr ammetr;

        #region События

        public event EventHandler UpdRowsTypesEvent;
        public event EventHandler UpdSensorInfoEvent;
        public event EventHandler UpdTestResultEvent;
        public event EventHandler ContinueRequest;
        public event EventHandler SelectionRequest;
        public event EventHandler<int> ProgressEvent;
        public event EventHandler UpdateMeasurmendIndicators;
        public event EventHandler ProcessComplete;

        #endregion


        public IDeviceSpecification DeviceSpecification { get; private set; }
        public ProductInfo Product{ get; private set; }
        public TestResults TestResults { get; private set; }
        public Exception Exception { get; private set; }
        public PressSystemInfo PressSystemInfo { get; private set; }
        public SystemStatus SystemStatus { get; private set; }
        
        public Settings Settings { get; }

        public Stand()
        {
            Settings = new Settings();
            Settings.Load();
            progress = new Progress();
            progress.ProgressChanged += (obj, val) => ProgressEvent(this, val);
            SystemStatus = new SystemStatus();
        }

        public void Init(IDialogService dialogService)
        {
            try
            {
                this.dialogService = dialogService;
                Exception = null;
                var psysCommands = new PsysCommandSimulator();
                psys = new PressSystem(psysCommands, 20);
                SystemStatus.Init(Settings);
                psys.ExceptionEvent += Exception_psys_event;
                psys.ConnectEvent += SystemStatus.PressSysten_ConnectEvent;
                // ammetr = new Ammetr(Settings.AmmetrSettins.Ip);
                
                savingResults = new SavingResults(Settings, SystemStatus);
                if (!Settings.UsedRemoteControl)
                {
                    processErrorHandler = new ErrorHandler(Settings, SystemStatus);
                    ReadPsysInfo();
                }
                else
                {
                    processErrorHandler = new ErrorHandlerRemoteControlMode(Settings, SystemStatus);
                }
                
            }
            catch(Exception ex)
            {
                Exception = ex;
                dialogService.ErrorMessage("Не удалось установить связь с пневмосистемой. Проверьте состояние готовности пневмосистемы " +
                                        "и нажмите кнопку \"Подключиться к пневмосистеме\". Или измените настройки в меню \"Система\"");
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
                Start(cts.Token);
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
        private void Start(ProductInfo productInfo, CancellationTokenSource cts)
        {
            try
            {
                this.cts = cts;
                Product = productInfo;
                IDevice device = Product.Device;
                Start(cts.Token);
                savingResults.SaveResult(Product, TestResults, dialogService);
            }

            catch (OperationCanceledException) { }
           
        }

        private void Start(CancellationToken cancellation)
        {
            try
            {
                IDevice device = Product.Device;
                ReadPsysInfo();
                DeviceSpecification.CheckRangeSupport(device);
                UpdSensorInfoEvent?.Invoke(this, new EventArgs());
                ammetr = new AmmetrSimulator(psys, device.Range.Min, device.Range.Max, 0.05, device.Range.RangeType == RangeTypeEnum.DA);
                ammetr.ExceptionEvent += Exception_ammetr_event;
                ammetr.ConnectEvent += SystemStatus.Ammetr_ConnectEvent;
                ammetr.StartCycleMeasureCurrent();
                measurmendIndicator = new MeasurmendIndicator(ammetr, psys, device.Range.RangeType == RangeTypeEnum.DA);
                measurmendIndicator.UpdDataEvent += UpdateMeasurmendIndicators;
                measurmendIndicator.Start();

                TestProcess testProcess;
                if (Settings.UsedAutomaticSortingOut)
                    testProcess = new TestProcess(psys, ammetr, GetTestPoints());
                else
                {
                    waitContinue = new WaitContinue();
                    waitContinue.ContinueRequest += ContinueRequest;
                    waitContinue.SelectionRequest += (obj, e) => SelectionRequest(this, e);
                    testProcess = new TestProcess(waitContinue.Wait, psys, ammetr, GetTestPoints());
                }
                testProcess.UpdResultsEvent += UpdTestResult_event;
                progress.Report(0);
                testProcess.RunProcess(device.Range.Min, device.Range.Max, device.ClassPrecision, GetPsysOutChannel(),
                        device.Range.RangeType == RangeTypeEnum.DA, cancellation, progress);
                if (TestResults.GetResume() != true)
                    Product.Error = ProcessErrorEnum.BadPrecision;
                Stop();
                waitContinue?.WaitSelection(cancellation);
            }
            catch (OperationCanceledException)
            {
                Stop();
                if (Exception != null)
                    processErrorHandler.ErrorHanding(Exception, Product, dialogService);
                else
                    throw;
            }
            catch(Exception ex)
            {
                Stop();
                Exception = ex;
                processErrorHandler.ErrorHanding(ex, Product, dialogService);
            }
        }

        // В режиме ручной разбраковки (о, кто придумал этот идиотизм!?) будет обрабатываться нажмтие кнопки "Далее"
        public void NextStep()
        {
            if (waitContinue != null)
                waitContinue.Continue();
        }

        public void Release()
        {
            Product.Error = ProcessErrorEnum.NoError;
            waitContinue.Continue();
        }

        public void Reject()
        {
            if (Product.Error == ProcessErrorEnum.NoError)
                Product.Error = ProcessErrorEnum.OperatorSolution;
            waitContinue.Continue();
        }

        public void ShowAboutTheProgramm()
        {
            var metrologicInfo = new MetrologicInfo();
            string info = $"Версия сборки v.{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}" +
                $"\n{new string('-', 80)}\n\n" + metrologicInfo.GetMetrologicInfo();
            
            dialogService.Message("О программе:", info);
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

        private double[] GetTestPoints()
        {
            return new double[] { 0, 0.25, 0.5, 0.75, 1 };
        }


        private int GetPsysOutChannel()
        {
            return 3;
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
