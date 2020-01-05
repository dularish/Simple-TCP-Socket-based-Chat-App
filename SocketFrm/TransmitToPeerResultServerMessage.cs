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
    public class TransmitToPeerResultServerMessage : ServerMessage
    {
        [DataMember]
        private string _ReceiverClientId;
        [DataMember]
        private int _SenderMessageId;
        [DataMember]
        private bool _IsReceivedByReceiver;

        public TransmitToPeerResultServerMessage(string receiverClientId, int senderMessageId, bool isReceivedByReceiver)
        {
            ReceiverClientId = receiverClientId;
            SenderMessageId = senderMessageId;
            IsReceivedByReceiver = isReceivedByReceiver;
            ServerMessageType = ServerMessageType.TransmitToPeerResult;
        }

        public string ReceiverClientId { get => _ReceiverClientId; private set => _ReceiverClientId = value; }
        public int SenderMessageId { get => _SenderMessageId; private set => _SenderMessageId = value; }
        public bool IsReceivedByReceiver { get => _IsReceivedByReceiver; private set => _IsReceivedByReceiver = value; }
    }
}
