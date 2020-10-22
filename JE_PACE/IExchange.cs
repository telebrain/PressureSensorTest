using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JE_PACE
{
    public interface IExchange
    {
        void Connect();

        void Disconnect();

        void Send(string message);

        string Request(string message);


    }
}
