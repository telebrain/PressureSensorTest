using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PressSystems
{
    public class PsysCommandSimulator : IPressSystemCommands
    {
        PressSystemVariables variables = new PressSystemVariables();
        // int i = 0;

        public void Connect(int outChannelNumber, CancellationToken cancellationToken)
        {
            Thread.Sleep(3000);
            // throw new Exception("Ошибка подключения к стойке давления");
        }

        int i = 0;
        public PressSystemVariables ReadSysVar()
        {
            // Операция чтения всех переменных: Pressure, Inlim, Barometr

            // Симуляция аварии
            
            //i++;
            //if (i >= 5)
            // throw new Exception("Нет связи с пневмосистемой");
            variables.Barometr = 99000;
            variables.TimeStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return variables;
        }

        public PressSystemInfo ReadInfo()
        {
            return CreatePressSystemInfo(); 
        }

        public void WriteSP(int controller, double SP, CancellationToken cancellationToken)
        {
            // Симуляция аварии
            //i++;
            //if (i >= 2)
            //    throw new Exception("Нет связи с пневмосистемой");

            ChangeSP(SP);
            // throw new Exception("Ошибка связи при установке давления");
        }

        public void Disconnect()
        {

        }

        private async void ChangeSP(double SP)
        {
            await Task.Run(() => {
                    variables.InLim = false;
                    Thread.Sleep(3000);
                    variables.InLim = true;
                    variables.Pressure = SP;
                }
            );
        }

        private PressSystemInfo CreatePressSystemInfo()
        {
            PressSystemInfo pressSystemInfo = new PressSystemInfo();
            pressSystemInfo.Controllers = new PressControllersList();

            pressSystemInfo.Controllers.Add(
                new PressControllerInfo()
                {
                    Number = 1,
                    IsEnabled = true,
                    SN = "00001",
                    RangeLo = 0,
                    RangeHi = 3500000,
                    Precision = 0.02F
                });

            pressSystemInfo.Controllers.Add(
                new PressControllerInfo()
                {
                    Number = 2,
                    IsEnabled = true,
                    SN = "00002",
                    RangeLo = 0,
                    RangeHi = 700000,
                    Precision = 0.01F
                });

            pressSystemInfo.Controllers.Add(
                new PressControllerInfo()
                {
                    Number = 3,
                    IsEnabled = true,
                    SN = "00003",
                    RangeLo = -100000,
                    RangeHi = 200000,
                    Precision = 0.02F
                });

            pressSystemInfo.Controllers.Add(
                new PressControllerInfo()
                {
                    Number = 4,
                    IsEnabled = true,
                    SN = "00004",
                    RangeLo = -35000,
                    RangeHi = 35000,
                    Precision = 0.02F
                });

            //pressSystemInfo.BarometrAvaiable = true;

            pressSystemInfo.RangeHi = 2800000;
            pressSystemInfo.RangeLo = -85000;

            return pressSystemInfo;

        }

        public void DisableControl(int controller)
        {
            throw new NotImplementedException();
        }

        public void DisableControl()
        {
            throw new NotImplementedException();
        }
    }
}
