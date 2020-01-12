using SocketFrm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm
{
    public interface IPeerMessageTransmitter
    {
        void QueueToTransmitPeerMessage(TransmitToPeerClientMessage clientMessage);
    }
}
