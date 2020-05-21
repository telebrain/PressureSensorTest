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
            mainWindow.Dispatcher.Invoke(() => MessageBox.Show(message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error));
        }

        public void ShowMessage(string message)
        {
            mainWindow.Dispatcher.Invoke(() => MessageBox.Show(message, "Внимание!", MessageBoxButton.OK, MessageBoxImage.None));
        }

        public void ShowMessage(string title, string message)
        {
            mainWindow.Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.None));
        }

        public bool ShowUserDialog(string message)
        {
            return mainWindow.Dispatcher.Invoke(() =>
            {
                MessageBoxResult result = MessageBox.Show(message, "Внимание", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                return (result == MessageBoxResult.OK);
            });
        }

        public bool ShowErrorDialog(string message)
        {
            return mainWindow.Dispatcher.Invoke(() => 
            {
                MessageBoxResult result = MessageBox.Show(message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                return (result == MessageBoxResult.OK);
            });
        }

        public void ShowWarningMessage(string message)
        {
            mainWindow.Dispatcher.Invoke(() => MessageBox.Show(message, "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning));
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
