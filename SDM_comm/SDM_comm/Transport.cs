using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace SDM_comm
{
    public class Transport: IDisposable
    {
        Socket sock;
        const int receiveTimeout = 5000;
        string ip;
        int port;


        public Transport(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

        }

        public void Connect()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.ReceiveTimeout = receiveTimeout;

            IAsyncResult result = sock.BeginConnect(ipEndPoint, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(3000, true);

            if (sock.Connected)
            {
                sock.EndConnect(result);
            }
            else
            {
                sock.Close();
                throw new Exception("Не удалось подключиться к серверу");
            }
        }

        public void Send(string sTx)
        {
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(sTx);
                sock.Send(msg);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка: " + e.Message + " --Send");
                throw e;
            }
        }

        public string Receive()
        {
            try
            {
                byte[] recv = new byte[1024];
                int byteRecv = sock.Receive(recv);
                string sRx = Encoding.ASCII.GetString(recv, 0, byteRecv);
                return sRx.Remove(sRx.Length - 1, 1);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка: " + e.Message + " --Receive");
                throw e;
            }
        }
        

        public void Dispose()
        {
            sock.Close();
            sock.Dispose();
        }
    }
}
