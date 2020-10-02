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
        const int RecBufferSize = 512;


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
                sock.ReceiveBufferSize = RecBufferSize;
            }
            else
            {
                sock.Close();
                throw new SDM_ErrException(Properties.Resources.ConnectError);
            }
        }

        public void Send(string sTx)
        {
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(sTx);
                sock.Send(msg);
                //System.Diagnostics.Debug.WriteLine("Передано: " + sTx);
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("Ошибка: " + e.Message + " --Send");
                throw e;
            }
        }

        public string Receive()
        {
            try
            {
                byte[] recv = new byte[RecBufferSize];
                int byteRecv = sock.Receive(recv);
                int shift = 0;
                for (int i = 0; i < byteRecv; i++)
                {
                    if (recv[i] != 0)
                    {
                        shift = i;
                        break;
                    }
                }
                string sRx = Encoding.ASCII.GetString(recv, shift, byteRecv);
                //System.Diagnostics.Debug.WriteLine("Принято: " + sRx);
                return sRx.Remove(sRx.Length - 1, 1);
            }
            catch (Exception e)
            {
                // System.Diagnostics.Debug.WriteLine("Ошибка: " + e.Message + " --Receive");
                throw e;
            }
        }
        

        public void Dispose()
        {
            sock.Close();
            // sock.Dispose();
        }
    }
}
