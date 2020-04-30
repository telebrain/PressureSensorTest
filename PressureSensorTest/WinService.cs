using System;
using System.Windows;

namespace PressureSensorTest
{
    public class WinService : IWinService
    {
        Window mainWindow = null;
        Window sysSettingsWin = null;
        Window settingsArchivingResultWin = null;

        public WinService()
        {
            mainWindow = Application.Current.Windows[0];
        }

        #region Диалоговые окна

        public void ShowErrorMessage(string message)
        {
            mainWindow.Dispatcher.Invoke(() => OpenWinErrorMessage(message));
        }

        public void ShowMessage(string message)
        {
            mainWindow.Dispatcher.Invoke(() => OpenWinMessage(message));
        }

        public bool ShowUserDialog(string message)
        {
            return mainWindow.Dispatcher.Invoke(() => OpenWinDialog(message));
        }

        public bool ShowErrorDialog(string message)
        {
            return mainWindow.Dispatcher.Invoke(() => OpenErrDialog(message));
        }

        public void ShowWarningMessage(string message)
        {
            mainWindow.Dispatcher.Invoke(() => OpenWinMessage(message));
        }

        private void OpenWinErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OpenWinMessage(string message)
        {
            MessageBox.Show(message, "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool OpenWinDialog(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Внимание", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            return (result == MessageBoxResult.OK);
        }

        private bool OpenErrDialog(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            return (result == MessageBoxResult.OK);
        }

        #endregion

        #region Окна настроек

        public void ShowSysSettingsWindow(object vm)
        {
            sysSettingsWin = new SysSettingsWindow()
            {
                DataContext = vm
            };
            sysSettingsWin.Owner = mainWindow;
            sysSettingsWin.ShowDialog();
        }

        public void CloseSysSettingsWindow()
        {
            sysSettingsWin.Close();
        }

        public void ShowArchivindSettingsWindow(object vm)
        {
            settingsArchivingResultWin = new SettingsArchivingResults()
            {
                DataContext = vm
            };
            settingsArchivingResultWin.Owner = mainWindow;
            settingsArchivingResultWin.ShowDialog();
        }

        public void CloseArchivindSettingsWindow()
        {
            settingsArchivingResultWin.Close();
        }

        


        #endregion
    }
}
