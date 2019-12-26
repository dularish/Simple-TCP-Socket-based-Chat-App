using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm
{
    [DataContract]
    public class TransmitToPeerClientMessage : ClientMessage
    {
        [DataMember]
        private string _TextMessage;

        public TransmitToPeerClientMessage(string textMessage, string receiverClientId, int senderMessageId)
        {
            TextMessage = textMessage;
            ReceiverClientId = receiverClientId;
            SenderMessageId = senderMessageId;
            ClientMessageType = ClientMessageType.TransmitToPeer;
        }

        public string TextMessage { get => _TextMessage; private set => _TextMessage = value; }
        public string ReceiverClientId { get => _ReceiverClientId; private set => _ReceiverClientId = value; }
        public int SenderMessageId { get => _SenderMessageId; private set => _SenderMessageId = value; }

        [DataMember]
        private int _SenderMessageId;
        [DataMember]
        private string _ReceiverClientId;
    }
}
