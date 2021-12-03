using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD_RS;
using PressureSensorTestCore;
using System.IO.Ports; 

namespace PressureSensorTest
{
    public class PD_RS_device: IDigitalPort
    {
        readonly string portName;
        SerialPort port;
        Commands commands;

        const int baudrate = 9600;
        const Parity parity = Parity.None;
        const int dataBits = 8;
        const StopBits stopBits = StopBits.One;
        const byte address = 16;
        double[] multiplers = new double[] { 1, 1e3, 1e6 };
        double multipler;

        public PD_RS_device(string portName)
        {
            this.portName = portName;   
        }

        public void Init()
        {
            port = new SerialPort(portName, baudrate, parity, dataBits, stopBits);
            commands = new Commands(port, address);
            PD_RS.UnitsEnum units = commands.GetUnits();
            multipler = multiplers[(int)units];
        }

        public double GetPressurePa()
        {
            return commands.ReadPressure() * multipler;
        }

        public void Dispose()
        {
            port.Close();
        }
    }
}
