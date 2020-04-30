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

        protected override void WriteSP(int channel, double SP)
        {
            try
            {
                // Симуляция аварии
                //i++;
                //if (i >= 2)
                //    throw new Exception("Нет связи с пневмосистемой");

                CurrentSP = SP;
                CurrentChannel = channel;
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
            pressSystemInfo.Controllers = new PressControllerInfo[4];

            pressSystemInfo.Controllers[0] = new PressControllerInfo();
            pressSystemInfo.Controllers[0].Enabled = true;
            pressSystemInfo.Controllers[0].SN = "00001";
            pressSystemInfo.Controllers[0].PressureLo = 0;
            pressSystemInfo.Controllers[0].PressureHi = 3500000;
            pressSystemInfo.Controllers[0].Precision = 0.02F;

            pressSystemInfo.Controllers[1] = new PressControllerInfo();
            pressSystemInfo.Controllers[1].Enabled = true;
            pressSystemInfo.Controllers[1].SN = "00002";
            pressSystemInfo.Controllers[1].PressureLo = 0;
            pressSystemInfo.Controllers[1].PressureHi = 700000;
            pressSystemInfo.Controllers[1].Precision = 0.01F;

            pressSystemInfo.Controllers[2] = new PressControllerInfo();
            pressSystemInfo.Controllers[2].Enabled = true;
            pressSystemInfo.Controllers[2].SN = "00003";
            pressSystemInfo.Controllers[2].PressureLo = -100000;
            pressSystemInfo.Controllers[2].PressureHi = 200000;
            pressSystemInfo.Controllers[2].Precision = 0.02F;

            pressSystemInfo.Controllers[3] = new PressControllerInfo();
            pressSystemInfo.Controllers[3].Enabled = true;
            pressSystemInfo.Controllers[3].SN = "00004";
            pressSystemInfo.Controllers[3].PressureLo = -35000;
            pressSystemInfo.Controllers[3].PressureHi = 35000;
            pressSystemInfo.Controllers[3].Precision = 0.02F;

            pressSystemInfo.BarometrAvaiable = true;

            pressSystemInfo.PressureHi = 2800000;
            pressSystemInfo.PressureLo = -85000;

            return pressSystemInfo;

        }

        
    }
}
