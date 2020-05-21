using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureRack
{
    public class PressureRackException: Exception
    {
        public int ErrorNumber { get; private set; } = -1;

        public override string Message { get; }

        public PressureRackException(string message)
        {
            Message = message;
            ErrorNumber = -1;
        }

        public PressureRackException(int errNumber)
        {
            try
            {
                ErrorNumber = errNumber;
                Properties.Resources.ResourceManager.GetString($"Err{errNumber}");
            }
            catch
            {
                Message = $"Неизвестный номер ошибки ({errNumber})";
            }
        }


    }
}
