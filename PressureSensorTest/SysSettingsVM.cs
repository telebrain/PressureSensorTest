using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Windows;
using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PressureSensorTest
{
    public class SysSettingsVM : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }


        IWinService winService;
        Settings settings;

        public SysSettingsVM(Settings settings, IWinService winService)
        {
            this.settings = settings;
            this.winService = winService;
            SettingsToVis();
            winService.ShowSysSettingsWindow(this);

        }


        #region Bildings

        public List<string> PressSourceList { get; } =
            new List<string>(new string[] { "Стойка эталонов давления", "JE PACE5000" });

        int pressSourceItem;
        public int PressSourceItem
        {
            get { return pressSourceItem; }
            set
            {
                pressSourceItem = value;
                OnPropertyChanged();
                if (pressSourceItem == 0)
                {
                    PaceSettingsVisibility = Visibility.Collapsed;
                    PressureRackSettingsVisibility = Visibility.Visible;
                }
                else
                {
                    PaceSettingsVisibility = Visibility.Visible;
                    PressureRackSettingsVisibility = Visibility.Collapsed;
                }
            }
        }

        Visibility paceSettingsVisibility = Visibility.Collapsed;
        public Visibility PaceSettingsVisibility
        {
            get { return paceSettingsVisibility; }
            set { paceSettingsVisibility = value; OnPropertyChanged(); }
        }

        Visibility pressureRackSettingsVisibility;
        public Visibility PressureRackSettingsVisibility
        {
            get { return pressureRackSettingsVisibility; }
            set { pressureRackSettingsVisibility = value; OnPropertyChanged(); }
        }

        List<string> portList;
        public List<string> PortList
        {
            get { return portList; }
            set { portList = value; OnPropertyChanged(); }
        }

        string pacePortName;
        public string PacePortName
        {
            get { return pacePortName; }
            set { pacePortName = value; OnPropertyChanged(); }
        }

        int paceSrcPlusValue;
        public int PaceSrcPlusValue
        {
            get { return paceSrcPlusValue; }
            set
            {
                paceSrcPlusValue = value;
                OnPropertyChanged();
                if (paceSrcPlusValue < 1 || paceSrcPlusValue > 10000)
                    AddValidError("Значение может быть только в диапазоне от 1 до 10 000 кПа");
                else
                    ClearValidError();
            }
        }

        int paceSrcMinusValue;
        public int PaceSrcMinusValue
        {
            get { return paceSrcMinusValue; }
            set
            {
                paceSrcMinusValue = value;
                OnPropertyChanged();
                if (paceSrcMinusValue < -110 || paceSrcPlusValue > 0)
                    AddValidError("Значение может быть только в диапазоне от 0 до -110 кПа");
                else
                    ClearValidError();
            }
        }

        public List<string> PacePrecissionList { get; } =
            new List<string>(new string[] { "0,02", "0,01" });

        string paceClassPrecision;
        public string PaceClassPrecision
        {
            get { return paceClassPrecision; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    paceClassPrecision = value;
                else
                    paceClassPrecision = PacePrecissionList[0];
                OnPropertyChanged();
            }
        }

        int paceMaxTimeSetPressure;
        public int PaceMaxTimeSetPressure
        {
            get { return paceMaxTimeSetPressure; }
            set
            {
                paceMaxTimeSetPressure = value;
                OnPropertyChanged();
                if (paceMaxTimeSetPressure < 1 || paceMaxTimeSetPressure > 360)
                    AddValidError("Значение может быть только в диапазоне от 1 до 360 с");
                else
                    ClearValidError();
            }
        }

        int paceTimeStabPressure;
        public int PaceTimeStabPressure
        {
            get { return paceTimeStabPressure; }
            set
            {
                paceTimeStabPressure = value;
                OnPropertyChanged();
                if (paceTimeStabPressure < 1 || paceTimeStabPressure > 30)
                    AddValidError("Значение может быть только в диапазоне от 1 до 30 с");
                else
                    ClearValidError();
            }
        }

        string psysIp;
        public string PsysIp
        {
            get { return psysIp; }
            set { psysIp = value; OnPropertyChanged(); }
        }

        int maxTimeSetPressure;
        public int MaxTimeSetPressure
        {
            get { return maxTimeSetPressure; }
            set
            {
                maxTimeSetPressure = value;
                OnPropertyChanged();
                if (maxTimeSetPressure < 1 || maxTimeSetPressure > 360)
                    AddValidError("Значение может быть только в диапазоне от 1 до 360 с");
                else
                    ClearValidError();
            }
        }

        int timeStabPressure;
        public int TimeStabPressure
        {
            get { return timeStabPressure; }
            set
            {
                timeStabPressure = value;
                OnPropertyChanged();
                if (timeStabPressure < 1 || timeStabPressure > 30)
                    AddValidError("Значение может быть только в диапазоне от 1 до 30 с");
                else
                    ClearValidError();
            }
        }

        int outChannelByThread_20;
        public int OutChannelByThread_20
        {
            get { return outChannelByThread_20; }
            set
            {
                outChannelByThread_20 = value;
                OnPropertyChanged();
                if (outChannelByThread_20 < 1 || outChannelByThread_20 > 8)
                    AddValidError("Значение может быть только в диапазоне от 1 до 8");
                else
                    ClearValidError();
            }
        }

        int outChannelByThread_12;
        public int OutChannelByThread_12
        {
            get { return outChannelByThread_12; }
            set
            {
                outChannelByThread_12 = value;
                OnPropertyChanged();
                if (outChannelByThread_12 < 1 || outChannelByThread_12 > 8)
                    AddValidError("Значение может быть только в диапазоне от 1 до 8");
                else
                    ClearValidError();
            }
        }

        int outChannelByThread_14;
        public int OutChannelByThread_14
        {
            get { return outChannelByThread_14; }
            set
            {
                outChannelByThread_14 = value;
                OnPropertyChanged();
                if (outChannelByThread_14 < 1 || outChannelByThread_14 > 8)
                    AddValidError("Значение может быть только в диапазоне от 1 до 8");
                else
                    ClearValidError();
            }
        }

        string sdmIp;
        public string SdmIp
        {
            get { return sdmIp; }
            set { sdmIp = value; OnPropertyChanged(); }
        }

        bool usedWithDataBase;
        public bool UsedWithDataBase
        {
            get { return usedWithDataBase; }
            set { usedWithDataBase = value; OnPropertyChanged(); }
        }

        bool usedRemouteControl;
        public bool UsedRemouteControl
        {
            get { return usedRemouteControl; }
            set { usedRemouteControl = value; OnPropertyChanged(); }
        }

        bool usedAutomaticSortingOut;
        public bool UsedAutomaticSortingOut
        {
            get { return usedAutomaticSortingOut; }
            set { usedAutomaticSortingOut = value; OnPropertyChanged(); }
        }

        string dbPath;
        public string DbPath
        {
            get { return dbPath; }
            set { dbPath = value; OnPropertyChanged(); }
        }

        List<string> ipNetCardsContent;
        public List<string> IpNetCardsContent
        {
            get { return ipNetCardsContent; }
            set { ipNetCardsContent = value; OnPropertyChanged(); }
        }

        string ipNetCard;
        public string IpNetCard
        {
            get { return ipNetCard; }
            set { ipNetCard = value; OnPropertyChanged(); }
        }

        bool showVariations;
        public bool ShowVariations
        {
            get { return showVariations; }
            set { showVariations = value; OnPropertyChanged(); }
        }

        int pause;
        public int Pause
        {
            get { return pause; }
            set
            {
                pause = value;
                OnPropertyChanged();
                if (pause < 10 || pause > 100)
                    AddValidError("Значение может быть только в диапазоне от 10 до 100 с");
                else
                    ClearValidError();
            }
        }

        #endregion

        #region Commands
        RelayCommand acceptCommand;
        public RelayCommand AcceptCommand
        {
            get { return acceptCommand ?? (acceptCommand = new RelayCommand(obj => Accept())); }
        }

        RelayCommand cancelCommand;
        public RelayCommand CancelCommand
        {
            get { return cancelCommand ?? (cancelCommand = new RelayCommand(obj => Cancel())); }
        }

        RelayCommand findDbCommand;
        public RelayCommand FindDbConnand
        {
            get { return findDbCommand ?? (findDbCommand = new RelayCommand(obj => FindDbFile())); }
        }


        // Валидация
        public string Error { get; private set; } = "";

        Dictionary<string, string> validErrors = new Dictionary<string, string>();

        public string this[string columnName] => validErrors.ContainsKey(columnName) ? validErrors[columnName] : null;

        private void AddValidError(string errorMessage, [CallerMemberName]string property = "") => validErrors[property] = errorMessage;

        private void ClearValidError([CallerMemberName]string property = "") => validErrors[property] = null;

        private bool IsValid
        {
            get
            {
                foreach (var value in validErrors.Values)
                {
                    if (value != null)
                        return false;
                }
                return true;
            }
        }

        #endregion

        private void Accept()
        {
            if (IsValid)
            {
                VisToSettins();
                settings.Save();
                winService.CloseSysSettingsWindow();
            }
            else
            {
                winService.ShowErrorMessage("Невозможно сохранить настройки. Не все параметры имеют корректное значение");
            }
        }

        private void Cancel()
        {
            winService.CloseSysSettingsWindow();
        }

        private void FindDbFile()
        {
            
            using (var ofd = new OpenFileDialog())
            {
                if (!string.IsNullOrEmpty(DbPath))
                    ofd.InitialDirectory = Path.GetDirectoryName(DbPath);
                else
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.Filter = "Файлы MS Access (*.accdb)|*.accdb";

                var result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    DbPath = ofd.FileName;
                }
            }
        }

        private void VisToSettins()
        {
            settings.PressSystemItem = (PressSystemEnum)PressSourceItem;
            settings.PsysSettings.IP = PsysIp;
            settings.PsysSettings.MaxTimeSetPressure = MaxTimeSetPressure;
            settings.PsysSettings.TimeStabilisation = TimeStabPressure;
            settings.PsysSettings.ChannelsOut = new int[] { OutChannelByThread_20, OutChannelByThread_12, outChannelByThread_14};
            settings.AmmetrSettins.Ip = SdmIp;
            settings.PathToDb = DbPath;
            settings.UsedStandDatabase = UsedWithDataBase;
            settings.UsedRemoteControl = UsedRemouteControl;
            settings.UsedAutomaticSortingOut = UsedAutomaticSortingOut;
            settings.RemoteControlIp = IpNetCard;

            settings.PaceSettings.PortName = PacePortName;
            settings.PaceSettings.MaxTimeSetPressure = PaceMaxTimeSetPressure;
            settings.PaceSettings.TimeStabilisation = PaceTimeStabPressure;
            settings.PaceSettings.SrcMinusValue = PaceSrcMinusValue*1000;
            settings.PaceSettings.SrcPlusValue = PaceSrcPlusValue*1000;
            settings.PaceSettings.ClassPrecision = PaceClassPrecision;

            settings.ShowVariation = ShowVariations;
            settings.TestPause100 = Pause;
        }

        private void SettingsToVis()
        {
            PressSourceItem = (int)settings.PressSystemItem;
            PsysIp = settings.PsysSettings.IP;
            MaxTimeSetPressure = settings.PsysSettings.MaxTimeSetPressure;
            TimeStabPressure = settings.PsysSettings.TimeStabilisation;
            OutChannelByThread_20 = settings.PsysSettings.ChannelsOut[0];
            OutChannelByThread_12 = settings.PsysSettings.ChannelsOut[1];
            OutChannelByThread_14 = settings.PsysSettings.ChannelsOut[2];
            SdmIp = settings.AmmetrSettins.Ip;
            DbPath = settings.PathToDb;
            UsedWithDataBase = settings.UsedStandDatabase;
            UsedRemouteControl = settings.UsedRemoteControl;
            UsedAutomaticSortingOut = settings.UsedAutomaticSortingOut;
            IpNetCardsContent = new List<string>();
            IpNetCard = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string addr = ip.Address.ToString();
                        IpNetCardsContent.Add(addr);
                        if (addr == settings.RemoteControlIp)
                            IpNetCard = addr;
                    }
                }
            }

            InitPortList(settings.PaceSettings.PortName);
            PaceMaxTimeSetPressure = settings.PaceSettings.MaxTimeSetPressure;
            PaceTimeStabPressure = settings.PaceSettings.TimeStabilisation;
            PaceSrcMinusValue = (int)(settings.PaceSettings.SrcMinusValue/1000);
            PaceSrcPlusValue = (int)(settings.PaceSettings.SrcPlusValue/1000);
            PaceClassPrecision = settings.PaceSettings.ClassPrecision;

            ShowVariations = settings.ShowVariation;
            Pause = settings.TestPause100;
        }

        private void InitPortList(string portName)
        {
            PortList = new List<string> (SerialPort.GetPortNames());
            if (PortList.Count == 0)
            {
                PacePortName = "";
                return;
            }
            foreach (string item in PortList)
            {
                if (item == portName)
                {
                    PacePortName = item;
                    // OnPropertyChanged("");
                    return;
                }
            }
            PacePortName = PortList[0];
        }
        
    }
}
