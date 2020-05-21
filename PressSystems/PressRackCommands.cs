using System;
using System.Threading.Tasks;
using System.Threading;
using PressureRack;

namespace PressSystems
{
    public class PressRackCommands: IPressSystemCommands
    {
        Commands commands;

        public PressRackCommands(string ip, int port)
        {
            commands = new Commands(ip, port);
        }

        public void Connect(int outChannelNumber, CancellationToken cancellationToken)
        {
            commands.ConnectTcp();
            commands.Connect(outChannelNumber, cancellationToken);
        }

        PressSystemVariables variables = new PressSystemVariables();
        public PressSystemVariables ReadSysVar()
        {
            var receiveVar = commands.ReadVar();
            variables.Pressure = receiveVar.Pressure * 1000;
            variables.InLim = receiveVar.InLim;
            variables.Barometr = receiveVar.Barometr * 1000;
            variables.TimeStamp = receiveVar.TimeStamp;           
            return variables;
        }

        public PressSystemInfo ReadInfo()
        {
            commands.ConnectTcp();

            PressSystemInfo info = new PressSystemInfo();
            var controllersInfo = commands.GetControllersState();
            info.Controllers = new PressControllersList();
            for (int i = 0; i < controllersInfo.Length; i ++)
            {
                info.Controllers.Add(new PressControllerInfo()
                {
                    Number = i + 1,
                    SN = controllersInfo[i].SN,
                    IsEnabled = controllersInfo[i].IsEnabled,
                    RangeHi = controllersInfo[i].RangeHi * 1000,
                    RangeLo = controllersInfo[i].RangeLo *1000,
                    Precision = controllersInfo[i].PrecClass
                });
            }

            commands.GetPressureRange(out double PressureLo, out double PressureHi);

            info.RangeLo = PressureLo;
            info.RangeHi = PressureHi;

            commands.Close();

            return info;
        }

        public void WriteSP(int controller, double SP, CancellationToken cancellationToken)
        {
            commands.WriteSP(controller, SP, cancellationToken);
        }

        public void Disconnect()
        {
            commands.Close();
        }


        public void DisableControl()
        {
            commands.DisableControl();
        }
    }

    
}
