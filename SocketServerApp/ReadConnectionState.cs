using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace SocketServerApp
{
    internal class ReadConnectionState
    {
        private TcpClient _TcpClient;
        private byte[] _DataSizeBuffer;

        public ReadConnectionState(TcpClient tcpClient)
        {
            _TcpClient = tcpClient;
            _DataSizeBuffer = new byte[4];
        }

        public byte[] DataSizeBuffer { get => _DataSizeBuffer; }
        public TcpClient TcpClient { get => _TcpClient; }
    }
}
