using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OwenPressureDevices;
using System.Windows;

namespace PressureSensorTest 
{
    public class MainVM : INotifyPropertyChanged
    {
        //const int Rows = 5;
        //const int Columns = 11;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region Привязки данных

        // Таблица результатов
        TableResultData tableResultData = new TableResultData();
        public TableResultData TableResultData
        {
            get { return tableResultData; }
            private set { tableResultData = value; OnPropertyChanged(); }
        }

        // Ряды для комбобоксов типа датчика

        List<string> titleDevice;
        public List<string> TitlesDevice
        {
            get { return titleDevice; }
            set { titleDevice = value; OnPropertyChanged(); }
        }

        int titleDeviceIndex;
        public int TitleDeviceIndex
        {
            get { return titleDeviceIndex; }
            set { titleDeviceIndex = value; OnPropertyChanged(); }
        }

        List<string> rangeTypeLabel;
        public List<string> RangeTypesLabels
        {
            get { return rangeTypeLabel; }
            set { rangeTypeLabel = value; OnPropertyChanged(); }
        }

        int rangeRangeTypeLabelIndex;
        public int RangeTypeLabelIndex
        {
            get { return rangeRangeTypeLabelIndex; }
            set
            {
                rangeRangeTypeLabelIndex = value;
                OnPropertyChanged();
                UpdRangeRow();
            }
        }

        List<string> rangeRow;
        public List<string> RangeRow
        {
            get { return rangeRow; }
            set { rangeRow = value; OnPropertyChanged(); }
        }

        int rangeRowIndex;
        public int RangeRowIndex
        {
            get { return rangeRowIndex; }
            set { rangeRowIndex = value; OnPropertyChanged(); }
        }

        List<string> modifications;
        public List<string> Modifications
        {
            get { return modifications; }
            set { modifications = value; OnPropertyChanged(); }
        }

        int modificationIndex;
        public int ModificationIndex
        {
            get { return modificationIndex; }
            set { modificationIndex = value; OnPropertyChanged(); }
        }

        List<string> threadTypes;
        public List<string> ThreadTypes
        {
            get { return threadTypes; }
            set { threadTypes = value; OnPropertyChanged(); }
        }

        int threadTypeIndex;
        public int ThreadTypeIndex
        {
            get { return threadTypeIndex; }
            set { threadTypeIndex = value; OnPropertyChanged(); }
        }

        List<string> classes;
        public List<string> Classes
        {
            get { return classes; }
            set { classes = value; OnPropertyChanged(); }
        }

        int classIndex;
        public int ClassIndex
        {
            get { return classIndex; }
            set
            {
                classIndex = value;
                OnPropertyChanged();
                UpdRangeRow();
            }
        }

        // Индикатор давления
        string pressure;
        public string Pressure
        {
            get { return pressure; }
            private set { pressure = value; OnPropertyChanged(); }
        }

        // Индикатор тока
        string current;
        public string Current
        {
            get { return current; }
            private set { current = value; OnPropertyChanged(); }

        }

        // Заводской номер
        string serialNumber = "";
        public string SerialNumber
        {
            get { return serialNumber; }
            set { serialNumber = value; OnPropertyChanged(); }
        }

        // Название датчика при выборе по базе
        string deviceName;
        public string DeviceName
        {
            get { return deviceName; }
            private set { deviceName = value; OnPropertyChanged(); }
        }

        // Прогрессбар
        int progress;
        public int Progress
        {
            get { return progress; }
            private set { progress = value; OnPropertyChanged(); }
        }

        // Показать индикатор успешного завершения теста
        Visibility showResultOK = Visibility.Hidden;
        public Visibility VisResultOK
        {
            get { return showResultOK; }
            private set { showResultOK = value; OnPropertyChanged(); }
        }

        // Показать индикатор отбраковки изделия
        Visibility showResultError = Visibility.Hidden;
        public Visibility VisResultError
        {
            get { return showResultError; }
            private set { showResultError = value; OnPropertyChanged(); }
        }

        Visibility sensorSelectionControlsVis;
        public Visibility SensorSelectionControlsVis
        {
            get { return sensorSelectionControlsVis; }
            set { sensorSelectionControlsVis = value; OnPropertyChanged(); }
        }

        Visibility readPsysInfoButtonVis;
        public Visibility ReadPsysInfoButtonVis
        {
            get { return readPsysInfoButtonVis; }
            set { readPsysInfoButtonVis = value; OnPropertyChanged(); }
        }

        Visibility sensorNameIndVisible;
        public Visibility SensorNameIndVisible
        {
            get { return sensorNameIndVisible; }
            set { sensorNameIndVisible = value; OnPropertyChanged(); }
        }

        bool snInputEnable;
        public bool SnInputEnable
        {
            get { return snInputEnable; }
            set { snInputEnable = value; OnPropertyChanged(); }
        }

        string startButtonText;
        public string StartButtonText
        {
            get { return startButtonText; }
            set { startButtonText = value; OnPropertyChanged(); }
        }

        bool startButtonEnable;
        public bool StartButtonEnable
        {
            get { return startButtonEnable; }
            set { startButtonEnable = value; OnPropertyChanged(); }
        }

        // Доступ к кнопке продолжить
        bool nextStepButtonEnable = false;
        public bool NextStepButtonEnable
        {
            get { return nextStepButtonEnable; }
            set { nextStepButtonEnable = value; OnPropertyChanged(); }
        }

        // Доступ к кнопкам "Брак" и "Годен"
        bool outButtonsEnable = false;
        public bool OutButtonsEnable
        {
            get { return outButtonsEnable; }
            set { outButtonsEnable = value; OnPropertyChanged(); }
        }

        // Блокировка и разблокировка доступа к некоторым элементам управления во время процесса
        bool unlockControl = true;
        public bool UnlockControl
        {
            get { return unlockControl; }
            set { unlockControl = value; OnPropertyChanged(); }
        }

        StateIndicator psysState = StateIndicator.Disabled;
        public StateIndicator PsysState
        {
            get { return psysState; }
            set { psysState = value; OnPropertyChanged(); }
        }

        StateIndicator ammetrState = StateIndicator.Disabled;
        public StateIndicator AmmetrState
        {
            get { return ammetrState; }
            set { ammetrState = value; OnPropertyChanged(); }
        }

        StateIndicator dbState = StateIndicator.Disabled;
        public StateIndicator DbState
        {
            get { return dbState; }
            set { dbState = value; OnPropertyChanged(); }
        }

        StateIndicator serverState = StateIndicator.Disabled;
        public StateIndicator ServerState
        {
            get { return serverState; }
            set { serverState = value; OnPropertyChanged(); }
        }

        string psysStateMessage;
        public string PsysStateMessage
        {
            get { return psysStateMessage; }
            set { psysStateMessage = value; OnPropertyChanged(); }
        }

        string ammetrStateMessage;
        public string AmmetrStateMessage
        {
            get { return ammetrStateMessage; }
            set { ammetrStateMessage = value; OnPropertyChanged(); }
        }

        string dbStateMessage;
        public string DbStateMessage
        {
            get { return dbStateMessage; }
            set { dbStateMessage = value; OnPropertyChanged(); }
        }

        string serverStateMessage;
        public string ServerStateMessage
        {
            get { return serverStateMessage; }
            set { serverStateMessage = value; OnPropertyChanged(); }
        }

        bool signalNextStepButton;
        public bool SignalNextStepButton
        {
            get { return signalNextStepButton; }
            set { signalNextStepButton = value; OnPropertyChanged(); }
        }

        bool signalRejectButton;
        public bool SignalRejectButton
        {
            get { return signalRejectButton; }
            set { signalRejectButton = value; OnPropertyChanged(); }
        }

        bool signalReleaseButton;
        public bool SignalReleaseButton
        {
            get { return signalReleaseButton; }
            set { signalReleaseButton = value; OnPropertyChanged(); }
        }

        #endregion

        #region Комманды


        RelayCommand startStopCommand;
        public RelayCommand StartStopCommand
        {
            get { return startStopCommand ?? (startStopCommand = new RelayCommand(obj => StartStopProcess())); }
        }

        RelayCommand nextStepCommand;
        public RelayCommand NextStepCommand
        {
            get { return nextStepCommand ?? (nextStepCommand = new RelayCommand(obj => NextStep())); }
        }

        RelayCommand rejectCommand;
        public RelayCommand RejectCommand
        {
            get { return rejectCommand ?? (rejectCommand = new RelayCommand(obj => Reject())); }
        }

        RelayCommand releaseCommand;
        public RelayCommand ReleaseCommand
        {
            get { return releaseCommand ?? (releaseCommand = new RelayCommand(obj => Release())); }
        }

        RelayCommand openSystemSettingsCommand;
        public RelayCommand OpenSystemSettingsCommand
        {
            get { return openSystemSettingsCommand ?? (openSystemSettingsCommand = new RelayCommand(obj => OpenWinSystemSettings())); }
        }

        RelayCommand openResultSettingsCommand;
        public RelayCommand OpenResultSettingsCommand
        {
            get { return openResultSettingsCommand ?? (openResultSettingsCommand = new RelayCommand(obj => OpenWinArchivingResultSettings())); }
        }

        RelayCommand openAboutTheProgrammCommand;
        public RelayCommand OpenAboutTheProgrammCommand
        {
            get { return openAboutTheProgrammCommand ?? (openAboutTheProgrammCommand = new RelayCommand(obj => stand.ShowAboutTheProgramm())); }
        }

        RelayCommand readPsysInfoCommand;
        public RelayCommand ReadPsysInfoCommand
        {
            get { return readPsysInfoCommand ?? (readPsysInfoCommand = new RelayCommand(obj => StandInit())); }
        }

        #endregion


        Stand stand = null;
        Settings settings;
        IDeviceSpecification specification;
        IWinService winService = new WinService();

        public MainVM()
        {
            stand = new Stand();
            settings = stand.Settings;
            settings.UpdSettingsEvent += UpdSettings_event;
            stand.UpdRowsTypesEvent += UpdRowsTypes_event;
            stand.UpdSensorInfoEvent += UpdSensorInfo_event;
            stand.UpdTestResultEvent += UpdTestResult_event;
            stand.UpdateMeasurmendIndicators += UpdIndicators_event;
            stand.ProgressEvent += Progress_event;
            stand.ContinueRequest += NextStepRequest_event;
            stand.SelectionRequest += SelectedRequest_event;
            stand.ProcessComplete += ProcessComplete_event;
            stand.SystemStatus.ChangeSysStatusEvent += SystemStatus_ChangeSysStatusEvent;
            ControlsToStopMode();
            StandInit();
        }

        private void SystemStatus_ChangeSysStatusEvent(object sender, EventArgs e)
        {
            var status = (SystemStatus)sender;

            PsysState = (StateIndicator)status.PressSystemStatus;
            PsysStateMessage = status.PressSystemStatusMessage;

            AmmetrState = (StateIndicator)status.AmmetrStatus;
            AmmetrStateMessage = status.AmmetrStatusMessage;

            DbState = (StateIndicator)status.DataBaseStatus;
            DbStateMessage = status.DataBaseStatusMessage;

            ServerState = (StateIndicator)status.ServerStatus;
            ServerStateMessage = status.ServerStatusMessage;
        }

        private void ProcessComplete_event(object sender, EventArgs e)
        {
            ShowResultIndicator();
        }

        private void ShowResultIndicator()
        {
            if (stand.Product.Error == ProcessErrorEnum.NoError)
            {
                VisResultError = Visibility.Hidden;
                VisResultOK = Visibility.Visible;               
            }
            else
            {
                VisResultOK = Visibility.Hidden;
                VisResultError = Visibility.Visible;
            }
        }

        private void StandInit()
        {
            IDialogService dialogService = new DialogService(winService);
            stand.Init(dialogService);
            StartButtonEnable = !settings.UsedRemoteControl;
            if (!settings.UsedRemoteControl)
            { 
                SensorNameIndVisible = Visibility.Hidden;
                if (stand.Exception == null)
                {
                    SensorSelectionControlsVis = Visibility.Visible;
                    ReadPsysInfoButtonVis = Visibility.Hidden;
                }
                else
                {
                    SensorSelectionControlsVis = Visibility.Hidden;
                    ReadPsysInfoButtonVis = Visibility.Visible;
                }
            }
            else
            { 
                SensorSelectionControlsVis = Visibility.Hidden;
                ReadPsysInfoButtonVis = Visibility.Hidden;
                SensorNameIndVisible = Visibility.Visible;
            }
        }

        bool runState = false;
        bool lockStart = false; // Флаг блокировки повторного старта до полного завершения процесса

        private void StartStopProcess()
        {
            if (!runState)
            {
                if (!lockStart)
                    Run();
            }
            else
                Cancel();
        }

        private void NextStepRequest_event(object sender, EventArgs e)
        {
            NextStepButtonEnable = true;
            SignalNextStepButton = true;
        }

        private void SelectedRequest_event(object sender, EventArgs e)
        {
            ShowResultIndicator();
            OutButtonsEnable = true;
            SignalReleaseButton = stand.TestResults.GetResume() == true;
            SignalRejectButton = !stand.TestResults.GetResume() == true;
        }

        private void NextStep()
        {
            stand.NextStep();
            NextStepButtonEnable = false;
            SignalNextStepButton = false;           
        }

        private void Reject()
        {
            stand.Reject();
            OutButtonsEnable = false;
            SignalReleaseButton = false;
            SignalRejectButton = false;
        }

        private void Release()
        {
            stand.Release();
            OutButtonsEnable = false;
            SignalReleaseButton = false;
            SignalRejectButton = false;
        }

        CancellationTokenSource cts;


        private async void Run()
        {
            OutButtonsEnable = false;
            cts = new CancellationTokenSource();
            var componentsName = new ComponentsOfDeviceName(TitlesDevice[TitleDeviceIndex], RangeTypesLabels[RangeTypeLabelIndex],
                RangeRow[RangeRowIndex], Modifications[ModificationIndex], ThreadTypes[ThreadTypeIndex], Classes[ClassIndex]);
            string deviceName = specification.ConcateName(componentsName);
            runState = true;
            ControlsToRunMode();           

            await Task.Run(() => stand.Start(SerialNumber, deviceName, cts));
            
            ControlsToStopMode();
            runState = false;
            lockStart = false;

        }
        private void Cancel()
        {
            lockStart = true;
            cts.Cancel();           
        }

        private void OpenWinSystemSettings()
        {
            SysSettingsVM sysSettingsVM = new SysSettingsVM(settings, winService);
            StandInit();
        }

        private void OpenWinArchivingResultSettings()
        {
            SettingsArchivingResultsVM vm = new SettingsArchivingResultsVM(settings, winService);
            StandInit();
        }

        private void UpdRangeRow()
        {
            string label = "";
            if (RangeRow != null)
                label = RangeRow[rangeRowIndex];
            RangeRow = new List<string>();
            RangeRow = specification.GetPressureRowLabels(RangeTypeLabelIndex, ClassIndex);
            RangeRowIndex = specification.GetIndexPressureRange(label, RangeTypeLabelIndex, ClassIndex);
            // OnPropertyChanged("");
        }

        private void UpdSettings_event(object sender, EventArgs e)
        {
            StartButtonEnable = !settings.UsedRemoteControl;
            StandInit();
        }

        private void UpdRowsTypes_event(object sender, EventArgs e)
        {
            specification = ((Stand)sender).DeviceSpecification;
            TitlesDevice = specification.Titles;
            RangeTypesLabels = specification.RangeTypesLabels;
            Modifications = specification.Modifications;
            ThreadTypes = specification.ThreadTypes;
            Classes = specification.Classes;
            UpdRangeRow();
        }

        PressureIndication pressureIndication;

        private void UpdSensorInfo_event(object sender, EventArgs e)
        {
            var stand = (Stand)sender;
            pressureIndication = new PressureIndication(stand.Product.Device.Range.Pressure_Pa);
        }

        private void UpdTestResult_event(object sender, EventArgs e)
        {
            TableResultData = new TableResultData(((Stand)sender).TestResults);
        }

        private void UpdIndicators_event(object sender, EventArgs e)
        { 
            var ind = (MeasurmendIndicator)sender;
            Pressure = pressureIndication.GetPressureWithUnit(ind.Pressure);
            Current = ind.Current.ToString("0.000") + " мА";
        }        

        private void Progress_event(object sender, int val)
        {
            Progress = val;
        }

        private void ControlsToRunMode()
        {
            StartButtonText = "СТОП";
            UnlockControl = false;
            VisResultOK = Visibility.Hidden;
            VisResultError = Visibility.Hidden;
        }

        private void ControlsToStopMode()
        {
            StartButtonText = "СТАРТ";
            UnlockControl = true;
            NextStepButtonEnable = false;
            SignalNextStepButton = false;
            OutButtonsEnable = false;
            SignalReleaseButton = false;
            SignalRejectButton = false;
            Pressure = "";
            Current = "";
        }
    }
}
