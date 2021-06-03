using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows;

namespace PressureSensorTest
{
    public class SettingsArchivingResultsVM : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string propery = "" )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propery));
        }

        string pathResult;
        public string PathResult
        {
            get { return pathResult; }
            set { pathResult = value; OnPropertyChanged(); }
        }

        bool usedFtp;
        public bool UsedFtp
        {
            get { return usedFtp; }
            set
            {
                usedFtp = value;
                OnPropertyChanged();
                EnblObservrtButton = !usedFtp;
                if (usedFtp)
                    FtpElementsVis = Visibility.Visible;
                else
                    FtpElementsVis = Visibility.Collapsed;
            }
        }

        bool enblObservrtButton;
        public bool EnblObservrtButton
        {
            get { return enblObservrtButton; }
            set { enblObservrtButton = value; OnPropertyChanged(); }
        }

        bool enableArchiving;
        public bool EnableArchiving
        {
            get { return enableArchiving; }
            set
            {
                enableArchiving = value;
               

                OnPropertyChanged();
            }
        }

        int standID;
        public int StandID
        {
            get { return standID; }
            set { standID = value; OnPropertyChanged(); }
        }

        string lineID;
        public string LineID
        {
            get { return lineID; }
            set { lineID = value; OnPropertyChanged(); }
        }

        string hardwareVer;
        public string HardwareVer
        {
            get { return hardwareVer; }
            set { hardwareVer = value; OnPropertyChanged(); }
        }

        int maxCommBreak;
        public int MaxComBreak
        {
            get { return maxCommBreak; }
            set
            {
                maxCommBreak = value;
                OnPropertyChanged();
                if (maxCommBreak < 0 || maxCommBreak > 24)
                    AddValidError("Значение должно быть в диапазоне от 0 до 24");
                else
                    ClearValidError();
            }
        }

        string login;
        public string Login
        {
            get { return login; }
            set { login = value; OnPropertyChanged(); }
        }

        string password;
        public string Password
        {
            get { return password; }
            set { password = value; OnPropertyChanged(); }
        }

        Visibility ftpElementsVis;
        public Visibility FtpElementsVis
        {
            get { return ftpElementsVis; }
            set { ftpElementsVis = value; OnPropertyChanged(); }
        }


        RelayCommand saveSettingsCommand;
        public RelayCommand SaveSettingsCommand
        {
            get { return saveSettingsCommand ?? (saveSettingsCommand = new RelayCommand(obj => Save())); }
        }

        RelayCommand cancelCommand;
        public RelayCommand CancelCommand
        {
            get { return cancelCommand ?? (cancelCommand = new RelayCommand(obj => Cancel())); }
        }

        RelayCommand findResultFolderCommand;
        public RelayCommand FindResultFolderCommand
        {
            get { return findResultFolderCommand ?? (findResultFolderCommand = new RelayCommand(obj => FindDbFile())); }
        }

        #region Валидация

        public string Error => throw new NotImplementedException();

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

        Settings settings;
        IWinService winService;

        public SettingsArchivingResultsVM(Settings settings, IWinService winService)
        {
            this.settings = settings;
            PathResult = settings.JsonReportSettings.ArchivingPath;
            UsedFtp = settings.JsonReportSettings.UsedFtp;
            EnableArchiving = settings.JsonReportSettings.ArchivingJsonFile;
            LineID = settings.JsonReportSettings.LineId;
            MaxComBreak = settings.JsonReportSettings.MaxCommunicationBreakWithArchive;
            HardwareVer = settings.JsonReportSettings.StandHardwareVer;
            StandID = settings.JsonReportSettings.StandId;
            Login = settings.JsonReportSettings.FtpLogin;
            Password = settings.JsonReportSettings.FtpPassword;
            this.winService = winService;
            winService.ShowArchivindSettingsWindow(this, settings.Password);

        }

        private void Save()
        {
            if (IsValid)
            {
                settings.JsonReportSettings.ArchivingPath = PathResult;
                settings.JsonReportSettings.UsedFtp = UsedFtp;
                settings.JsonReportSettings.ArchivingJsonFile = EnableArchiving;
                settings.JsonReportSettings.LineId = LineID;
                settings.JsonReportSettings.MaxCommunicationBreakWithArchive = MaxComBreak;
                settings.JsonReportSettings.StandHardwareVer = HardwareVer;
                settings.JsonReportSettings.StandId = StandID;
                settings.JsonReportSettings.FtpLogin = Login;
                settings.JsonReportSettings.FtpPassword = Password;
                settings.Save();
                winService.CloseArchivindSettingsWindow();
            }
            else
            {
                winService.ShowErrorMessage("Невозможно сохранить настройки. Не все параметры имеют корректное значение");
            }
        }

        private void Cancel()
        {
            winService.CloseArchivindSettingsWindow();
        }

        private void FindDbFile()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = PathResult;
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK & !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    PathResult = fbd.SelectedPath;
                }
            }
        }
    }
}
