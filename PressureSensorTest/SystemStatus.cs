using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTest
{
    public class SystemStatus
    {
        Settings settings;

        public event EventHandler ChangeConnectPressSystemEvent;
        public PressSystemsConnectEnum PressSystemsStateConnect { get; set; }

        public void PressSysten_ConnectEvent(object sender, EventArgs e)
        {
            PressSystemStatus = StatusEnum.Ok;
            PressSystemsStateConnect = PressSystemsConnectEnum.EndConnect;
            ChangeConnectPressSystemEvent?.Invoke(this, new EventArgs());
        }

        public void PressSystem_BeginConnectEvent(object sender, EventArgs e)
        {
            PressSystemsStateConnect = PressSystemsConnectEnum.BeginConnect;
            ChangeConnectPressSystemEvent?.Invoke(this, new EventArgs());
        }

        public void PressSystemDisconnectEvent(object sender, EventArgs e)
        {
            PressSystemsStateConnect = PressSystemsConnectEnum.Disconnect;
            ChangeConnectPressSystemEvent?.Invoke(this, new EventArgs());
        }

        public void Ammetr_ConnectEvent(object senser, EventArgs e)
        {
            AmmetrStatus = StatusEnum.Ok;
        }

        public event EventHandler ChangeSysStatusEvent;

        const string Undefined = "Состояние не определено";
        const string OK = "Нет ошибок";
        const string Disabled = "Выключено";

        StatusEnum pressSystemStatus = StatusEnum.Disabled;
        public StatusEnum PressSystemStatus
        {
            get { return pressSystemStatus; }
            set
            {
                pressSystemStatus = value;
                if (pressSystemStatus == StatusEnum.Disabled)
                    PressSystemStatusMessage = Undefined;
                if (pressSystemStatus == StatusEnum.Ok)
                    PressSystemStatusMessage = OK;
                ChangeSysStatus();
            }
        }

        public string PressSystemStatusMessage { get; set; }

        StatusEnum ammetrStatus = StatusEnum.Disabled;
        public StatusEnum AmmetrStatus
        {
            get { return ammetrStatus; }
            set
            {
                ammetrStatus = value;
                if (ammetrStatus == StatusEnum.Disabled)
                    AmmetrStatusMessage = Undefined;
                if (ammetrStatus == StatusEnum.Ok)
                    AmmetrStatusMessage = OK;
                ChangeSysStatus(); }
        }

        public string AmmetrStatusMessage { get; set; }

        StatusEnum dataBaseStatus = StatusEnum.Disabled;
        public StatusEnum DataBaseStatus
        {
            get { return dataBaseStatus; }
            set
            {
                dataBaseStatus = value;
                if (dataBaseStatus == StatusEnum.Disabled)
                    DataBaseStatusMessage = settings.UsedStandDatabase ? Undefined : Disabled;
                if (dataBaseStatus == StatusEnum.Ok)
                    DataBaseStatusMessage = OK;
                ChangeSysStatus();
            }
        }

        public string DataBaseStatusMessage { get; set; }

        StatusEnum serverStatus = StatusEnum.Disabled;
        public StatusEnum ServerStatus
        {
            get { return serverStatus; }
            set
            {
                serverStatus = value;
                if (serverStatus == StatusEnum.Disabled)
                    ServerStatusMessage = settings.UsedStandDatabase ? Undefined : Disabled;
                if (serverStatus == StatusEnum.Ok)
                    ServerStatusMessage = OK;
                ChangeSysStatus();
            }
        }

        public string ServerStatusMessage { get; set; }

        public void Init(Settings settings)
        {
            this.settings = settings;
            PressSystemStatus = StatusEnum.Disabled;
            AmmetrStatus = StatusEnum.Disabled;
            DataBaseStatus = StatusEnum.Disabled;
            ServerStatus = StatusEnum.Disabled;
        }

        private void ChangeSysStatus()
        {
            ChangeSysStatusEvent?.Invoke(this, new EventArgs());
        }
    }

   
    public enum StatusEnum { Disabled = -1, Ok = 0, Error, Warning }
    public enum PressSystemsConnectEnum { Disconnect = -1, BeginConnect = 0, EndConnect = 1 }

}

