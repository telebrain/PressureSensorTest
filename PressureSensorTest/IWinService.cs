namespace PressureSensorTest
{
    public interface IWinService
    {
        void CloseArchivindSettingsWindow();
        void CloseSysSettingsWindow();
        void ShowArchivindSettingsWindow(object vm);
        void ShowErrorMessage(string message);
        void ShowMessage(string message);
        void ShowSysSettingsWindow(object vm);
        bool ShowUserDialog(string message);
        bool ShowErrorDialog(string message);
        void ShowWarningMessage(string message);
    }
}