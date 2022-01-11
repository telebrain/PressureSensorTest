using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace TCPserver
{
    public class Server
    {
        readonly static ILog log = LogManager.GetLogger(typeof(Server));

        IRequestHandler requestHandler;
        readonly int clientsNumber = 1;
        readonly ClientsList clientsList = null;
        readonly int receiveTimeout = 3;

        public event EventHandler ErrorMessageEvent;
        readonly Encoding encoding = Encoding.Unicode;

        public class ErrorMessageEventArgs : EventArgs
        {
            public string Message { get; }
            public ErrorMessageEventArgs(string message)
            {
                Message = message;
            }
        }

        public Server(IRequestHandler requestHandler, int clientsNumber, int receiveTimeout)
        {
            this.requestHandler = requestHandler;
            this.clientsNumber = clientsNumber;
            this.receiveTimeout = receiveTimeout;
            clientsList = new ClientsList(clientsNumber);
        }

        public Server(IRequestHandler requestHandler, int clientsNumber, int receiveTimeout, Encoding encoding)
        {
            this.requestHandler = requestHandler;
            this.clientsNumber = clientsNumber;
            this.receiveTimeout = receiveTimeout;
            clientsList = new ClientsList(clientsNumber);
            this.encoding = encoding;
        }

        public async Task Listening(IPAddress addr, int port, CancellationToken cancellationToken)
        {
            
            try
            {
                IPEndPoint endPoint = new IPEndPoint(addr, port);
                Socket listner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listner.Bind(endPoint);
                listner.Listen(clientsNumber);
                while (true)
                {
                    await Task.Run(() => AcceptAsync(listner, cancellationToken), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);                
                ErrorMessageEvent?.Invoke(this, new ErrorMessageEventArgs(ex.Message));
            }
        }

        private async Task AcceptAsync(Socket listner, CancellationToken cancellationToken)
        {
            try
            {
                var socket = await Task.Run(() => Accept(listner), cancellationToken);
                var client = new Client(socket, requestHandler, receiveTimeout, encoding);
                if (clientsList.Add(client))
                    client.ExchangeAsync(cancellationToken).GetAwaiter();
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private Task<Socket> Accept(Socket listner)
        {
            TaskCompletionSource<Socket> tcs = new TaskCompletionSource<Socket>();
            listner.BeginAccept((iasyncState) =>
            {
                try
                {
                    var handler = listner.EndAccept(iasyncState);
                    tcs.TrySetResult(handler);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, null);
            return tcs.Task;
        }
    }
}
