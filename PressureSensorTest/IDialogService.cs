using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTest
{
    public interface IDialogService
    {
        void ErrorMessage(string message);

        void Message(string message);

        void WarningMessage(string message);

        bool TwoButtonDialog(string message);

        bool ErrorTwoButtonDialog(string message);

    }
}
