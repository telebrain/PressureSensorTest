using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

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
            //sock.Connect(ipEndPoint);

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

        private void Send(string sTx)
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

        private string Receive()
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


        public void OnlySend(string sTx)
        {
            Send(sTx);
        }

        public string OnlyReceive()
        {
            return Receive();
        }

        //public string Exch(string sTx)
        //{
        //    byte[] msg = Encoding.ASCII.GetBytes(sTx);
        //    sock.Send(msg);
        //    string sRx = "";
        //    byte[] recv = new byte[512];
        //    int byteRecv = sock.Receive(recv);
        //    sRx = Encoding.ASCII.GetString(recv, 0, byteRecv);
        //    sRx = sRx.Remove(sRx.Length - 1, 1);
        //    return sRx;
        //}

        public void Dispose()
        {
            sock.Close();
            sock.Dispose();
        }
    }
}
