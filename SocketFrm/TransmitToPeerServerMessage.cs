using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm
{
    [Serializable]
    [DataContract]
    public class TransmitToPeerServerMessage : ServerMessage
    {
        [DataMember]
        private TransmitToPeerClientMessage _TransmitToPeerClientMessage;

        public TransmitToPeerServerMessage(TransmitToPeerClientMessage transmitToPeerClientMessage)
        {
            TransmitToPeerClientMessage = transmitToPeerClientMessage;
            ServerMessageType = ServerMessageType.TransmitToPeer;
        }

        public TransmitToPeerClientMessage TransmitToPeerClientMessage { get => _TransmitToPeerClientMessage; private set => _TransmitToPeerClientMessage = value; }
    }
}
