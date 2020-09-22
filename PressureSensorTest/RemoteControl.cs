using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using TCPserver;
using OwenPressureDevices;

namespace PressureSensorTest
{
    public class RemoteControl: IDisposable
    {
        readonly Stand stand;
        ReqHandler reqHandler;
        Server server = null;
        CancellationTokenSource cts;
        Task t;
        readonly IPAddress address;
        readonly int port;

        public int ProductStatus
        {
            get
            {
                if (stand.Product != null && StateProcess == 0)
                {
                    return (int)stand.Product.Error;
                }
                else
                {
                    return -100;
                }
            }
        }
        public bool SystemError
        {
            get
            {
                return stand.SystemStatus.AmmetrStatus == StatusEnum.Error ||
                    stand.SystemStatus.PressSystemStatus == StatusEnum.Error ||
                    stand.SystemStatus.DataBaseStatus == StatusEnum.Error ||
                    stand.SystemStatus.ServerStatus == StatusEnum.Error;

            }
        }


        public StateProcessEnum StateProcess { get; private set; } = StateProcessEnum.Indefined;

        public RemoteControl(Stand stand, string ip, int port)
        {
            this.stand = stand;
            reqHandler = new ReqHandler(this);
            server = new Server(reqHandler, 1, 10000);
            if (!IPAddress.TryParse(ip, out address))
                address = IPAddress.Parse("127.0.0.1");
            this.port = port;
            stand.StopEvent += Stand_ProcessComplete;

            // server.ErrorMessageEvent += Server_ErrorMessageEvent;
            
        }

        private void Stand_ProcessComplete(object sender, EventArgs e)
        {
            if (stand.Product.Error != TestErrorEnum.InDefined)
                StateProcess = StateProcessEnum.OkComplete;
            else
                StateProcess = StateProcessEnum.Cancelled;
        }

        public void StartListening()
        {
            cts = new CancellationTokenSource();
            t = server.Listening(address, port, cts.Token);          
        }

        private void Server_ErrorMessageEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void StartProcess(ProductRequest productReq)
        {
            try
            {
                
                CheckSerialNumber(productReq.SN);
                CheckBoxNumber(productReq.Box);
                var dt = ParseDateTime(productReq.DateOp);
                IDevice device = GetDevice(productReq.SN, productReq.Name);
                StateProcess = StateProcessEnum.Started;
                Task t = stand.RemoteStart(device, productReq.Box, dt, productReq.PrimaryVerification);
                t.GetAwaiter();
            }
            catch (ParseException) { }
        }

        public void Cancel()
        {
            stand.RemoteCancel();
        }

        public void Dispose()
        {
            cts.Cancel();
            t.GetAwaiter();
        }

        private IDevice GetDevice(string sn, string name)
        {
            try
            {
                return new PD100_Device(sn, name);
            }
            catch
            {
                StateProcess = StateProcessEnum.ParsingNameError;
                throw new ParseException();
            }
        }

        private void CheckSerialNumber(string sn)
        {
            if (sn.Length < 17 || sn.Length > 19)
            {
                StateProcess = StateProcessEnum.ParsingSnError;
                throw new ParseException();
            }
        }

        private void CheckBoxNumber(string box)
        {
            if (!(box.Length == 4 && int.TryParse(box, out int val)))
            {
                StateProcess = StateProcessEnum.ParsingBoxError;
                throw new ParseException();
            }

        }

        private DateTime ParseDateTime(string dt)
        {
            if (DateTime.TryParse(dt, out DateTime dateTime))
            {
                return dateTime;
            }
            else
            {
                StateProcess = StateProcessEnum.ParsingDateTimeError;
                throw new ParseException();
            }
        }

        
    }

    public class ParseException: Exception { }

    public enum StateProcessEnum
    {
        Indefined = -100, // Неопределенное состояние     
        Started = -1, // Поверка выполняется
        OkComplete = 0, // Завершено без ошибок
        Cancelled = 1, // Завершено с отменой
        ParsingSnError = 2, // Неверен формат серийного номера
        ParsingBoxError = 3, // Неверен формат номера оснастки
        ParsingNameError = 4, // Не удалось разобрать имя продукта
        ParsingDateTimeError = 5, // Не удалось преобразовать формат времени
         
    }

    public enum ProductStatus
    {
        Indefined = -100, // Неопределенное состояние
        OkComplete = 0, // Завершено без ошибок
        BadPrecision = 14, // Не соответсвует метрологическим характеристикам
        Leakage = 15, // Утечка
        SystemError = 16, // Брак из-за неисправности оборудования
        RangeNotSupportByPsys = 17, // Диапазон изделия не поддерживается системой
        OperatorSolution = 18 // В брак по решению оператора
    }
    
}
