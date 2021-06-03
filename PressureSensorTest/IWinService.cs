namespace PressureSensorTest
{
    public interface IWinService
    {
        void CloseArchivindSettingsWindow();
        void CloseSysSettingsWindow();
        void ShowArchivindSettingsWindow(object vm);
        void ShowArchivindSettingsWindow(object vm, string pw);
        void ShowErrorMessage(string message);
        void ShowMessage(string message);
        void ShowMessage(string title, string message);
        void ShowSysSettingsWindow(object vm);
        void ShowSysSettingsWindow(object vm, string pw);
        bool ShowUserDialog(string message);
        bool ShowErrorDialog(string message);
        void ShowWarningMessage(string message);
    }
}