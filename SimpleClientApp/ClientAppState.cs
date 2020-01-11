using SocketFrm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClientApp
{
    internal class ClientAppState
    {
        private ConcurrentQueue<ClientMessage> _clientMessagesQueue = new ConcurrentQueue<ClientMessage>();
        private IClientUINotifier _clientUINotifier;
        private List<string> _availableUsers = new List<string>();
        private string _clientId = "Unnamed";

        public ClientAppState(IClientUINotifier consoleNotifier)
        {
            this._clientUINotifier = consoleNotifier;
        }

        public ConcurrentQueue<ClientMessage> ClientMessagesQueue { get => _clientMessagesQueue; set => _clientMessagesQueue = value; }
        public IClientUINotifier ClientUINotifier { get => _clientUINotifier; set => _clientUINotifier = value; }
        public List<string> AvailableUsers { get => _availableUsers; set => _availableUsers = value; }
        public string ClientId { get => _clientId; set => _clientId = value; }
        public TcpClient TCPClient { get; internal set; }
    }
}
