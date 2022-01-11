using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPserver
{
    public interface IRequestHandler
    {
        // Возвращает размер буфера принимаемого сообщения
        int ReceiveBuffer { get; }
        // Возвращает true, если строка принята полностью или false, если нужно принять остаток
        // Возвращает ответную строку на передачу
        string Handle(string received);
        // Сервер вызывает этот метод при пропадении связи с клиентом 
        void LostCustomerConnection();
        // Событие вызывается, когда от клиента приходит сообщение о разрыве соединения
        event EventHandler ClosingConnection;
    }
}
