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
    internal class ClientAppState : IPeerMessageTransmitter
    {
        private ConcurrentQueue<ClientMessage> _clientMessagesQueue = new ConcurrentQueue<ClientMessage>();

        internal ConcurrentQueue<ClientMessage> ClientMessagesQueue { get => _clientMessagesQueue; set => _clientMessagesQueue = value; }
        
        internal TcpClient TCPClient { get; set; }

        public void QueueToTransmitPeerMessage(TransmitToPeerClientMessage clientMessage)
        {
            ClientMessagesQueue.Enqueue(clientMessage);
        }
    }
}
