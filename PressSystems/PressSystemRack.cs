using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PressSystems
{
    //public class PressSystemRack: AbstractPressSystem
    //{
    //    // У стойки давления все параметры давления в кПа. Для адаптпции нужно переводить Па в кПа и обратно

    //    const int PortPressRack = 49002;
        

    //    PressureRack.PressSystemRack psys;
        

    //    public PressSystemRack(string ip, int maxTimeSetPressure): base(maxTimeSetPressure)
    //    {
    //        psys = new PressureRack.PressSystemRack(ip, PortPressRack);
    //        psys.UpdPsysVarEvent += UpdateMeasures;
    //        psys.ExceptionEvent += ExceptionEvent;
    //    }

    //    public override double Pressure { get { return psys.PV * 1000; } }

    //    public override double CurrentSP { get; protected set; }

    //    public override double Barometr { get { return psys.Bar * 1000; } }

    //    public override Exception Exception { get { return psys.CurrentException; } }

    //    // public bool ConnectState { get; private set; }

    //    public override event EventHandler UpdateMeasures;
    //    public override event EventHandler ExceptionEvent;


    //    public override void Connect(int outChannelNumber)
    //    {
    //        psys.StartPsys(outChannelNumber);
    //        CurrentOutputChannel = outChannelNumber;
    //    }

    //    public override void Disconnect()
    //    {
            
    //    }

    //    public override void ReadInfo()
    //    {
    //        psys.ReadPressSystemInfo();
    //        Info = new PressRackInfo(psys.PressSystemInfo);
    //    }        

    //    public void SetPressure(float SP, CancellationToken token)
    //    {
    //        SetPressure(CurrentChannel, SP, MaxTimeSetPressureOperation, token);
    //    }

    //    public void SetPressure(int channel, float SP, CancellationToken token)
    //    {
    //        SetPressure(channel, SP, MaxTimeSetPressureOperation, token);
    //    }

    //    public override void WriteNewSP(int channel, double SP)
    //    {
    //        float sp = (float)SP / 1000;
    //        psys.WriteSP(sp, channel);
    //        CurrentChannel = channel;
    //        CurrentSP = sp;
    //    }
    //}

    

    

}
