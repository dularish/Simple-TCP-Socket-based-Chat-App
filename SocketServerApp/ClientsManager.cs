using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerApp
{
    public class ClientsManager
    {
        private List<Client> _ClientsConnected = new List<Client>();

        public void AcceptClient(TcpClient tcpClient)
        {
            Client newClient = new Client(tcpClient);

            newClient.ClientDisconnected += OnClientDisconnected;
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _ClientsConnected.Remove(e.ClientDisconnected);
        }
    }
}
