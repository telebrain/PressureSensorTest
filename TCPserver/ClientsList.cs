using System;
using System.Collections.Generic;
using log4net;


namespace TCPserver
{
    public class ClientsList
    {
        readonly static ILog log = LogManager.GetLogger(typeof(ClientsList));

        List<Client> clients = new List<Client>();
        public int MaxCountList { get; } = -1;

        public ClientsList() { }

        public ClientsList(int maxCountList)
        {
            MaxCountList = maxCountList;
        }

        public bool Add(Client client)
        {
            if (MaxCountList == -1 || clients.Count < MaxCountList)
            {
                System.Diagnostics.Debug.WriteLine($"Добавление клиента Socket={client.Socket.GetHashCode()}");
                System.Diagnostics.Debug.WriteLine($"Число клиентов {clients.Count}");
                client.DisconnectEvent += Remove;
                clients.Add(client);
                return true;
            }
            else
            {
                client.Close();
                System.Diagnostics.Debug.WriteLine($"Клиент Socket={client.Socket.GetHashCode()} не добавлен");
                System.Diagnostics.Debug.WriteLine($"Число клиентов {clients.Count}");
                log.Error($"Клиент Socket ={ client.Socket.GetHashCode()} не добавлен. Число клиентов {clients.Count}");
                return false;
            }
        }

        public void Clear()
        {
            foreach (var client in clients)
            {
                client.Close();
            }
            clients.Clear();
        }

        private void Remove(object sender, EventArgs e)
        {
            clients.Remove((Client)sender);
            System.Diagnostics.Debug.WriteLine($"Клиент Socket={((Client)sender).Socket.GetHashCode()} удален");
            System.Diagnostics.Debug.WriteLine($"Число клиентов {clients.Count}");
        }

    }
}
