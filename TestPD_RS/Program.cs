using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD_RS;
using System.IO.Ports;

namespace TestPD_RS
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
            port.ReadTimeout = 2000;
            var commands = new Commands(port, 16);
            while(true)
            {
                System.Diagnostics.Debug.WriteLine(commands.ReadPressure());
            }
        }
    }
}
