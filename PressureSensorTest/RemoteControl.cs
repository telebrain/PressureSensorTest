using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using TCPserver;
using OwenPressureDevices;
using log4net;


namespace PressureSensorTest
{
    public class RemoteControl: IDisposable
    {
        readonly static ILog log = LogManager.GetLogger(typeof(RemoteControl)); 
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

        MetrologicGroups metrologicGroops;

        public StateProcessEnum StateProcess { get; private set; } = StateProcessEnum.Indefined;

        public RemoteControl(Stand stand, string ip, int port, Encoding encoding, MetrologicGroups metrologicGroops)
        {
            this.stand = stand;
            this.metrologicGroops = metrologicGroops;
            reqHandler = new ReqHandler(this);
            server = new Server(reqHandler, 1, 10000, encoding);
            if (!IPAddress.TryParse(ip, out address))
                address = IPAddress.Parse("127.0.0.1");
            this.port = port;
            stand.StopEvent += Stand_ProcessComplete;
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
            log.Info($"Запуск прослушки IP: <{address.ToString()}>, порт: <{port}>");
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
                // Task t = stand.RemoteStart(device, productReq.Box, dt, productReq.PrimaryVerification);
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
            string cirName = "";
            try
            {
                cirName = DecodeName(name);
                var device =  new PD100_Device(sn, cirName);
                device.MetrologicGroupNumber = metrologicGroops.GetMetrologicGroup(device);
                return device;
            }
            catch (PressureSensorTest.MetrologicGroupNotFounException)
            {
                StateProcess = StateProcessEnum.MetrologicGroupNotFound;
                log.Error($"Не удалось разобрать название ПД: <{name}>, <{cirName}>");
                throw;
            }

            catch(Exception ex)
            {
                log.Error($"Ошибка парсинга имени: {ex.ToString()}; {ex.Message}");
                StateProcess = StateProcessEnum.ParsingNameError;
                throw new ParseException();
            }
        }

        private string DecodeName(string name)
        {
            string nameOut = name;
            string[] codeLa = new string[] { "PD100I", "PD", "DIV", "DI", "DV", "DG", "DA", "DD" };
            string[] codeRus = new string[] { "ПД100И", "ПД", "ДИВ", "ДИ", "ДВ", "ДГ", "ДА", "ДД" };
            for (int i = 0; i < codeLa.Length; i++)
                nameOut = nameOut.Replace(codeLa[i], codeRus[i]);
            return nameOut;
        }

        private void CheckSerialNumber(string sn)
        {
            if (sn.Length < 17 || sn.Length > 19)
            {
                StateProcess = StateProcessEnum.ParsingSnError;
                log.Error($"Неверный формат серийного номера: <{sn}>");         
                throw new ParseException();
            }
        }

        private void CheckBoxNumber(string box)
        {
            if (!(box.Length == 4 && int.TryParse(box, out int val)))
            {
                StateProcess = StateProcessEnum.ParsingBoxError;
                log.Error($"Неверный формат номера оснастки: <{box}>");
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
                log.Error($"Неверный формат времени: <{dt}>");
                StateProcess = StateProcessEnum.ParsingDateTimeError;
                throw new ParseException();
            }
        }

        
    }

    public class ParseException: Exception { }

    public class MetrologicGroupNotFounException: Exception { }

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
        MetrologicGroupNotFound = 6 // Не найдена метрологическая группа
    }

    public enum ProductStatus
    {
        Indefined = -100, // Неопределенное состояние
        OkComplete = 0, // Завершено без ошибок
        BadPrecision = 14, // Не соответсвует метрологическим характеристикам
        Leakage = 15, // Утечка
        SystemError = 16, // Брак из-за неисправности оборудования
        RangeNotSupportByPsys = 17, // Диапазон изделия не поддерживается системой
        OperatorSolution = 18, // В брак по решению оператора
        BadVariation = 19, // Значение вариации больше допустимого
        AlarmLoLimit = 20, // Датчик в аварийном состоянии, ток меньше 4 мА
        AlarmHiLimit = 21 // Датчик в аварийном состоянии, ток больше 20 мА
    }
    
}
