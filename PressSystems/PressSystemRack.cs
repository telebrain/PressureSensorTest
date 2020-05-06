using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PressSystems
{
    public class PressSystemRack : AbstractPressSystem
    {
        // У стойки давления все параметры давления в кПа. Для адаптпции нужно переводить Па в кПа и обратно

        const int PortPressRack = 49002;


        readonly PressureRack.PressSystemRack psys;


        public PressSystemRack(string ip, int maxTimeSetPressure) : base(maxTimeSetPressure)
        {
            psys = new PressureRack.PressSystemRack(ip, PortPressRack);
        }

        protected override void ConnectOperation(int outChannelNumber, CancellationToken cancellationToken)
        {
            psys.Connect(outChannelNumber, cancellationToken);
        }

        protected override void ReadInfoCore()
        {
            psys.ReadPressSystemInfo();
            Info = new PressRackInfo(psys.PressSystemInfo);
        }

        protected override void ReadSysVar()
        {
            psys.ReadVariablesPsys(out double pressure, out double barometr, out bool inLim, out long timestmp);
            Pressure = pressure * 1000;
            Barometr = barometr * 1000;
            InLim = inLim;
            Timestamp = timestmp;
        }

        protected override void WriteSP(int controller, double SP, CancellationToken cancellationToken)
        {
            psys.WriteSP(SP * 0.001, controller, cancellationToken);
        }
    }
}
