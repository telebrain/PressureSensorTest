using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPserver;
using Newtonsoft.Json;

namespace RemoteControl
{
    public class ReqHandler : IRequestHandler
    {
        public int ReceiveBuffer { get; } = 512;

        public event EventHandler StartProcessEvent;
        public event EventHandler CancellationEvent;
        public event EventHandler ClosingConnection;

        public StateProcessEnum StateProcess { get; set; } = StateProcessEnum.Indefined;

        public string Handle(string received)
        { 
            System.Diagnostics.Debug.WriteLine("Received: " + received + "\n");
            string toSend = "";
            try
            {
                var request = (SimpleRequest)JsonConvert.DeserializeObject(received, typeof(SimpleRequest));
                if (request.Title == "TestStart")
                {
                    ProductRequest productRequest = (ProductRequest)JsonConvert.DeserializeObject(received, typeof(ProductRequest));
                    toSend = TestStartReqHandle(productRequest);
                }
                else if (request.Title == "ReqStateProcess")
                {
                    toSend = StateProcessReqHandle(request);
                }
                else if (request.Title == "Cancel")
                {
                    CancelReqHandle();
                }
                else
                {
                    toSend = (new SimpleRequest("Error", 2)).GetJsonFormat();
                }
                System.Diagnostics.Debug.WriteLine("ToSend: " + toSend + "\n");
                return toSend;
            }
            catch
            {
                return "";
            }
        }

        public void LostCustomerConnection()
        {
            CancellationEvent?.Invoke(this, new EventArgs());
            StateProcess = StateProcessEnum.Cancelled;
        }

        private string TestStartReqHandle(ProductRequest request)
        {
            string result = "";
            int statusMessage = 0;
            var newProduct = ProductInfoEventArgs.Parse((ProductRequest)request);
            if (newProduct != null)
            {
                if (StateProcess != StateProcessEnum.ProcessStarted)
                {
                    StateProcess = StateProcessEnum.ProcessStarted;
                    StartProcessEvent?.Invoke(this, newProduct);
                }
            }
            else
            {
                statusMessage = 2;
            }
            result = (new SimpleRequest(request.Title, statusMessage)).GetJsonFormat();
            return result;
        }

        private string StateProcessReqHandle(SimpleRequest request)
        {
            return (new SingleValueRequest(request.Title, 0, ((int)StateProcess).ToString())).GetJsonFormat();
        }

        private void CancelReqHandle()
        {
            ClosingConnection?.Invoke(this, new EventArgs());
            CancellationEvent?.Invoke(this, new EventArgs());
            StateProcess = StateProcessEnum.Cancelled;
        }
    }

    public enum StateProcessEnum
    {
        Indefined = -100, // Неопределенное состояние
        ParsingSnError = - 6, // Неверен формат серийного номера
        ParsingBoxError = -5, // Неверен формат номера оснастки
        NotFoundInDb = -4, // Продукт не найден в базе данных
        ParsingNameError = -3, // Не удалось разобрать имя продукта
        Cancelled = -2, // Завершено с отменой
        ProcessStarted = -1, // Поверка выполняется
        OkComplete = 0, // Завершено без ошибок
        BadPrecision = 14, // Не соответсвует метрологическим характеристикам
        Leakage = 15, // Утечка
        SystemError = 16, // Брак из-за неисправности оборудования
        RangeNotSupportByPsys = 17, // Диапазон изделия не поддерживается системой
        OperatorSolution = 18 // В брак по решению оператора
    }
}
