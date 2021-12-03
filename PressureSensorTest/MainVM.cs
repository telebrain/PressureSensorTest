using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OwenPressureDevices;
using System.Windows;
using System.Windows.Media;

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
            set
            {
                titleDeviceIndex = value >= 0 ? value : 0;
                OnPropertyChanged();
            }
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
                rangeRangeTypeLabelIndex = value >= 0 ? value : 0;
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
            set
            {
                rangeRowIndex = value >= 0 ? value : 0;
                OnPropertyChanged();
            }
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
            set
            {
                modificationIndex = value >= 0 ? value : 0;
                OnPropertyChanged();
            }
        }

        List<string> threadTypes;
        public List<string> ThreadTypes
        {
            get { return threadTypes; }
            set
            {
                threadTypes = value;
                OnPropertyChanged();
            }
        }

        int threadTypeIndex;
        public int ThreadTypeIndex
        {
            get { return threadTypeIndex; }
            set
            {
                threadTypeIndex = value >= 0 ? value : 0;
                OnPropertyChanged();
            }
        }

        List<string> modification2;
        public List<string> Modification2
        {
            get { return modification2; }
            set
            {
                modification2 = value;
                OnPropertyChanged();
            }
        }

        int modification2Index;
        public int Modification2Index
        {
            get { return modification2Index; }
            set
            {
                modification2Index = value;
                OnPropertyChanged();
            }
        }

        List<string> outPortType;
        public List<string> OutPortType
        {
            get { return outPortType; }
            set
            {
                outPortType = value;
                OnPropertyChanged();
            }
        }

        int outPortTypeIndex;
        public int OutPortTypeIndex
        {
            get { return outPortTypeIndex; }
            set
            {
                outPortTypeIndex = value;
                OnPropertyChanged();
            }
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
                classIndex = value >= 0 ? value : 0;
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

        Visibility deviceNameIndVisible;
        public Visibility DeviceNameIndVisible
        {
            get { return deviceNameIndVisible; }
            set { deviceNameIndVisible = value; OnPropertyChanged(); }
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

        Visibility showStateConnectPsys = Visibility.Hidden;
        public Visibility ShowStateConnectPsys
        {
            get { return showStateConnectPsys; }
            set { showStateConnectPsys = value; OnPropertyChanged(); }
        }

        string messageStateConnectPsys;
        public string MessageStateConnectPsys
        {
            get { return messageStateConnectPsys; }
            set { messageStateConnectPsys = value; OnPropertyChanged(); }
        }

        Brush colorStateConnectPsys;
        public Brush ColorStateConnectPsys
        {
            get { return colorStateConnectPsys; }
            set { colorStateConnectPsys = value; OnPropertyChanged(); }
        }

        Visibility tableVisibility;
        public Visibility TableVisibility
        {
            get { return tableVisibility; }
            set { tableVisibility = value; OnPropertyChanged(); }
        }

        Visibility shortTableVisibility;
        public Visibility ShortTableVisibility
        {
            get { return shortTableVisibility; }
            set { shortTableVisibility = value; OnPropertyChanged(); }
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
            stand.UpdProductInfoEvent += UpdProductInfo_event;
            stand.UpdTestResultEvent += UpdTestResult_event;
            stand.UpdateMeasurmendIndicators += UpdIndicators_event;
            stand.ProgressEvent += Progress_event;
            stand.ContinueRequest += NextStepRequest_event;
            stand.SelectionRequest += SelectedRequest_event;
            stand.ProcessComplete += ProcessComplete_event;
            stand.SystemStatus.ChangeSysStatusEvent += SystemStatus_ChangeSysStatusEvent;
            stand.SystemStatus.ChangeConnectPressSystemEvent += SystemStatus_ChangeConnectPressSystemEvent;
            stand.RemoteStartEvent += Stand_RemoteStartEvent;
            stand.StopEvent += Stand_RemoteStopEvent;
            ControlsToStopMode();
            StandInit();
        }

        private void Stand_RemoteStopEvent(object sender, EventArgs e)
        {
            ControlsToStopMode();
        }

        private void Stand_RemoteStartEvent(object sender, EventArgs e)
        {
            ControlsToRunMode();
        }

        private void SystemStatus_ChangeConnectPressSystemEvent(object sender, EventArgs e)
        {
            switch (stand.SystemStatus.PressSystemsStateConnect)
            {
                case PressSystemsConnectEnum.BeginConnect:
                    ShowStateConnectPsys = Visibility.Visible;
                    MessageStateConnectPsys = "Ожидание подключения к стойке давления";
                    ColorStateConnectPsys = Brushes.Yellow;
                    break;
                case PressSystemsConnectEnum.EndConnect:
                    ShowStateConnectPsys = Visibility.Visible;
                    MessageStateConnectPsys = "Подключение к стойке давления установлено";
                    ColorStateConnectPsys = Brushes.Lime;
                    break;
                default:
                    ShowStateConnectPsys = Visibility.Hidden;
                    MessageStateConnectPsys = "";
                    break;

            }

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
            ShowResultIndicator(stand.Product.Error == TestErrorEnum.NoError);
        }

        private void ShowResultIndicator(bool result)
        {
            if (result)
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
                DeviceNameIndVisible = Visibility.Hidden;
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
                DeviceNameIndVisible = Visibility.Visible;
            }
            ShowTable();
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
            bool result = stand.TestResults.GetResume() == 0;
            ShowResultIndicator(result);
            OutButtonsEnable = true;
            SignalReleaseButton = result;
            SignalRejectButton = !result;
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
            var name = new DeviceName(TitlesDevice[TitleDeviceIndex], RangeTypesLabels[RangeTypeLabelIndex],
                RangeRow[RangeRowIndex], Modifications[ModificationIndex], ThreadTypes[ThreadTypeIndex], Modification2[Modification2Index], 
                    Classes[ClassIndex], OutPortType[OutPortTypeIndex]);
            var device = new PD100_Device(SerialNumber, name);
            runState = true;
            ControlsToRunMode();           

            await Task.Run(() => stand.Start(device, cts));
            
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
        }

        private void OpenWinArchivingResultSettings()
        {
            SettingsArchivingResultsVM vm = new SettingsArchivingResultsVM(settings, winService);
        }

        private void UpdRangeRow()
        {
            string label = "";
            if (RangeRow != null)
                label = RangeRow[rangeRowIndex];
            RangeRow = new List<string>();
            RangeRow = specification.GetPressureRowLabels(RangeTypeLabelIndex, ClassIndex);
            RangeRowIndex = specification.GetIndexPressureRange(label, RangeTypeLabelIndex, ClassIndex);
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
            Modification2 = specification.Modifications2;
            ThreadTypes = specification.ThreadTypes;
            Classes = specification.Classes;
            OutPortType = specification.OutPortType;
            UpdRangeRow();
        }

        PressureIndication pressureIndication;

        private void UpdProductInfo_event(object sender, EventArgs e)
        {
            var product = ((Stand)sender).Product;
            pressureIndication = new PressureIndication(product.Device.Range.Pressure_Pa);
            SerialNumber = product.Device.SerialNumber;
            DeviceName = product.Device.Name.Name;
        }

        private void UpdTestResult_event(object sender, EventArgs e)
        {
            TableResultData = new TableResultData(((Stand)sender).TestResults);
        }

        private void UpdIndicators_event(object sender, EventArgs e)
        { 
            var ind = (MeasurmendIndicator)sender;
            Pressure = pressureIndication.GetPressureWithUnit(ind.Pressure);
            Current = ind.Current != null ? ind.Current.Value.ToString("0.0000") + " мА" : "";
        }        

        private void Progress_event(object sender, int val)
        {
            Progress = val;
        }

        private void ControlsToRunMode()
        {
            if (!settings.UsedRemoteControl)
            {
                StartButtonText = "СТОП";
                UnlockControl = false;
            }
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

        private void ShowTable()
        {
            if (settings.ShowVariation)
            {
                TableVisibility = Visibility.Visible;
                ShortTableVisibility = Visibility.Collapsed;
            }
            else
            {
                TableVisibility = Visibility.Collapsed;
                ShortTableVisibility = Visibility.Visible;
            }
        }
    }
}
