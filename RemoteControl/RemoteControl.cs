using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl
{
    public class RemoteControl : IRemoteControl
    {
        public event EventHandler StartProcessEvent;
        public event EventHandler CancellationEvent;


        readonly string ip;

        public RemoteControl(string serverIp)
        {
            ip = serverIp;
        }

        public void SendError(Exception exception)
        {
            
        }

        public void SendResult(bool state)
        {
            
        }
    }
}
