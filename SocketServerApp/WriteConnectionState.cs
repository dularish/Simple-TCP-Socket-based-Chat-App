using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerApp
{
    class WriteConnectionState
    {
        private TcpClient _tcpClient;
        private int _sentDataLength;

        public WriteConnectionState(TcpClient tcpClient, int sentDataLength)
        {
            _tcpClient = tcpClient;
            _sentDataLength = sentDataLength;
        }

        public TcpClient TcpClient { get => _tcpClient; }
        public int SentDataLength { get => _sentDataLength; }
    }
}
