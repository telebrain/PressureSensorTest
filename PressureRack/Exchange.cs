using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace PressureRack
{
    class Exchange: IDisposable
    {
        Socket sock;
        const int receiveTimeout = 3000;
        readonly string ip;
        readonly int port;

        internal Exchange(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        internal void ConnectTcp()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            
            sock.Connect(ipEndPoint);
            sock.ReceiveTimeout = receiveTimeout;
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
                return "";
            }
        }


        internal void OnlySend(string sTx)
        {
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(sTx);
                sock.Send(msg);
                return;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка: " + e.Message + " --OnlySend");
                return;
            }
        }

        internal string Exch(string sTx)
        {
            byte[] msg = Encoding.ASCII.GetBytes(sTx);
            sock.Send(msg);
            string sRx = "";
            byte[] recv = new byte[1024];
            int byteRecv = sock.Receive(recv);
            sRx = Encoding.ASCII.GetString(recv, 0, byteRecv);
            sRx = sRx.Remove(sRx.Length - 1, 1);
            return sRx;
        }

        public void Dispose()
        {
            sock.Close();
        }
    }
}
