using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace JE_PACE
{
    public class ComExchange : IExchange
    {
        public string PortName { get; set; }

        SerialPort port = null;

        const int baudrate = 115200;
        const Parity parity = Parity.None;
        const int databits = 8;
        const StopBits stopBits = StopBits.One;
        const int readTimeout = 3000;
        const string terminator = "\r";

        public ComExchange(string portName)
        {
            PortName = portName;
        }

        public void Connect()
        {
            port = new SerialPort(PortName, baudrate, parity, databits, stopBits)
            {
                ReadTimeout = readTimeout
            };
            port.Open();
        }

        public void Disconnect()
        {
            port.Close();
        }

        public string Request(string message)
        {
            Send(message);
            string receive = Read();
            receive = receive.Replace(terminator, "");
            return receive;
        }

        public void Send(string message)
        {
            port.DiscardInBuffer();
            port.WriteLine(message);
        }
        public string Read()
        {
            return port.ReadLine();
        }
    }
}
