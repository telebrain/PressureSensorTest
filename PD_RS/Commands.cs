using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System.IO.Ports;

namespace PD_RS
{
    public class Commands
    {
        IModbusSerialMaster master;
        readonly byte address;

        public Commands(SerialPort serialPort, byte address)
        {
            master = ModbusSerialMaster.CreateRtu(serialPort);
            if (!serialPort.IsOpen)
                serialPort.Open();
            this.address = address;
        }

        public float ReadPressure()
        {
            ushort[] receivedData = master.ReadHoldingRegisters(address, 2200, 2);
            return GetFloatParameter(receivedData);
        }

        public void WriteUnits(UnitsEnum units)
        {
            master.WriteMultipleRegisters(address, 5301, new ushort[] { (ushort) units });
        }

        public UnitsEnum GetUnits()
        {
            return (UnitsEnum)(master.ReadHoldingRegisters(address, 5301, 1))[0];
        }

        private float GetFloatParameter(ushort[] receivedData)
        {
            byte[] data = new byte[4];
            data[0] = (byte)(receivedData[1] & 0xFF);
            data[1] = (byte)((receivedData[1] >> 8) & 0xFF);
            data[2] = (byte)(receivedData[0] & 0xFF);
            data[3] = (byte)((receivedData[0] >> 8) & 0xFF);

            return BitConverter.ToSingle(data, 0);
        }

        
    }

    public enum UnitsEnum { Pa = 0, KPa, Mpa }
}
