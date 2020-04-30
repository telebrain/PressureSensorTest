using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTest
{
    public class DialogService : IDialogService
    {
        IWinService winService;

        public DialogService(IWinService winService)
        {
            this.winService = winService;
        }

        public void ErrorMessage(string message)
        {
            winService.ShowErrorMessage(message);
        }

        public bool ErrorTwoButtonDialog(string message)
        {
            return winService.ShowErrorDialog(message);
        }

        public void Message(string message)
        {
            winService.ShowMessage(message);
        }

        public bool TwoButtonDialog(string message)
        {
            return winService.ShowUserDialog(message);
        }

        public void WarningMessage(string message)
        {
            winService.ShowWarningMessage(message);
        }
    }
}
