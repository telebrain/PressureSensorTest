using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace TCPserver
{
    public class Client
    {
        readonly static ILog log = LogManager.GetLogger(typeof(Client));

        public bool StateConnect { get; private set; }

        public event EventHandler DisconnectEvent;

        public Socket Socket { get; }
        readonly IRequestHandler requestHandler;
        readonly int receiveTimeout = -1;
        readonly Encoding encoding = Encoding.Unicode;

        public Client(Socket socket, IRequestHandler requestHandler, int receiveTimeout):
             this(socket, requestHandler, receiveTimeout, Encoding.Unicode){}

        public Client(Socket socket, IRequestHandler requestHandler, int receiveTimeout, Encoding encoding)
        {
            try
            { 
                this.Socket = socket;
                this.requestHandler = requestHandler;
                this.receiveTimeout = receiveTimeout;
                this.requestHandler.ClosingConnection += RequestHandler_ClosingConnection;
                this.encoding = encoding;
                StateConnect = true;
                    // log.Info($"Подключен клиент. Формат данных {encoding.ToString()}");
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка добавления клиента: {ex.Message})");
            }
        }

        private void RequestHandler_ClosingConnection(object sender, EventArgs e)
        {
            Close();
        }

        public void Close()
        {
            if (StateConnect)
            {
                Socket.Close();
                Socket.Dispose();
                StateConnect = false;
                requestHandler.LostCustomerConnection();
                DisconnectEvent?.Invoke(this, new EventArgs());
            }
        }

        public async Task ExchangeAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (StateConnect)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Run(() => ExchangeAsyncCore(cancellationToken));
                }
            }
            catch(OperationCanceledException)
            {
                Close();
            }
            catch(Exception ex)
            {
                log.Error($"Работа с клиентом прервана: {ex.Message}");
                Close();
            }
        }

        private async Task ExchangeAsyncCore(CancellationToken cancellation)
        {
            CancellationTokenSource timerCts = new CancellationTokenSource();
            using (CancellationTokenSource lincedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, timerCts.Token))
            {
                string receivedStr = "";
                using (System.Timers.Timer timer = new System.Timers.Timer(receiveTimeout))
                {
                    timer.AutoReset = false;
                    timer.Elapsed += (obj, e) =>
                    {
                        timerCts.Cancel();
                        Close();
                    };
                    timer.Start();
                    while(!lincedCts.Token.IsCancellationRequested)
                    {                       
                        receivedStr = await ReadMessage(lincedCts.Token);
                        if (!string.IsNullOrEmpty(receivedStr))
                        {
                            timer.Stop();
                            break;
                        }
                    }
                }
                
                lincedCts.Token.ThrowIfCancellationRequested();
                // log.Info($"Принято сообщение: {receivedStr}");
                string toSend = requestHandler.Handle(receivedStr);
                // log.Info($"Отправлено сообщение: {toSend}");
                if (!string.IsNullOrEmpty(toSend))
                    Socket.Send(encoding.GetBytes(toSend));
            }
        }

        private async Task<string> ReadMessage(CancellationToken token)
        {
            // Если есть что читать, читаем...
            if (Socket.Available > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                do
                {
                    token.ThrowIfCancellationRequested();
                    string receivedStr = await Task.Run(() => Receive(Socket), token);
                    stringBuilder.Append(receivedStr);
                    // Thread.Sleep(50);
                    //... пока есть что читать
                }
                while (CheckReceiveBuffer());
                return stringBuilder.ToString();
            }
            return "";
        }

        private Task<string> Receive(Socket socket)
        {
            byte[] buffer = new byte[requestHandler.ReceiveBuffer];
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            try
            {
                socket.BeginReceive(buffer, 0, buffer.Length, 0, (iasyncResult) =>
                {
                    try
                    {
                        int bytesRead = socket.EndReceive(iasyncResult);
                        // System.Diagnostics.Debug.WriteLine("Принято байт: " + bytesRead);
                        tcs.TrySetResult(encoding.GetString(buffer, 0, bytesRead));
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
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            return tcs.Task;
        }

        
        private bool CheckReceiveBuffer()
        {
            const int WaitReceiveContinue = 50;
            using (System.Timers.Timer timer = new System.Timers.Timer(WaitReceiveContinue))
            {
                bool timeout = false;

                timer.AutoReset = false;
                timer.Elapsed += (obj, e) => timeout = true;
                timer.Start();
                while(true)
                {
                    if (timeout)
                        return false;
                    if (Socket.Available > 0)
                        return true;
                }
            }
        }
    }
}
