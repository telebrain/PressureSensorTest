using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using TCPserver;


namespace RemoteControl
{
    public class RemoteContr : IRemoteControl
    {
        public event EventHandler StartProcessEvent;
        public event EventHandler CancellationEvent;

        ReqHandler reqHandler;
        Server server = null;
        CancellationTokenSource cts;
        Task t;
        readonly IPAddress address;
        readonly int port;

        public RemoteContr(string ip, int port)
        {
            try
            {
                if (!IPAddress.TryParse(ip, out address))
                    address = IPAddress.Parse("127.0.0.1");
                this.port = port;
                reqHandler = new ReqHandler();
                reqHandler.StartProcessEvent += (o, e) => StartProcessEvent(o, e);
                reqHandler.CancellationEvent += (o, e) => CancellationEvent(o, e);
                server = new Server(reqHandler, 1, 3000);              
            }
            catch (OperationCanceledException) { }
            
        }

        public void StartListening()
        {
            cts = new CancellationTokenSource();
            t = server.Listening(address, port, cts.Token);
        }

        public void ChangeState(StateProcessEnum stateProcess)
        {
            reqHandler.StateProcess = stateProcess;
        }

        public void Dispose()
        {
            cts.Cancel();
            t.GetAwaiter().GetResult();
        }
    }
}
