using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PressSystems
{
    public class PressSystemSimulator : AbstractPressSystem
    {
        public PressSystemSimulator(int maxTimeSetPressure): base(maxTimeSetPressure)
        {

        }

        protected override void ConnectOperation(int outChannelNumber, CancellationToken token)
        {
            // Реализация операции подключения
        }

        public override void Disconnect()
        {
            base.Disconnect();
        }

        protected override void ReadInfoCore()
        {
            Info = CreatePressSystemInfo();
        }

        protected override void WriteSP(int channel, double SP, CancellationToken token)
        {
            try
            {
                // Симуляция аварии
                //i++;
                //if (i >= 2)
                //    throw new Exception("Нет связи с пневмосистемой");

                CurrentSP = SP;
                CurrentController = channel;
                ChangeSP(SP);
            }
            catch (Exception ex)
            {
                Exception = ex;
                throw;
            }
        }


        // int i = 0;
        protected override void ReadSysVar()
        {
            Timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            // Операция чтения всех переменных: Pressure, Inlim, Barometr

            // Симуляция аварии
            //i++;
            //if (i >= 5)
            //    throw new Exception("Нет связи с пневмосистемой");
        }

        private async void ChangeSP(double SP)
        {
            await Task.Run(() => {
                    InLim = false;
                    Thread.Sleep(3000);
                    InLim = true;
                    Pressure = SP;
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

        
    }
}
