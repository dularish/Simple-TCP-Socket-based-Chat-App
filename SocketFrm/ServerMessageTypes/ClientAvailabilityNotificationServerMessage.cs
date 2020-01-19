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
    public class ClientAvailabilityNotificationServerMessage : ServerMessage
    {
        [DataMember]
        private string _ClientUniqueId;

        public ClientAvailabilityNotificationServerMessage(string clientUniqueId, bool isAvailable)
        {
            ClientUniqueId = clientUniqueId;
            IsAvailable = isAvailable;
            ServerMessageType = ServerMessageType.ClientAvailabilityNotification;
        }

        public string ClientUniqueId { get => _ClientUniqueId; private set => _ClientUniqueId = value; }
        public bool IsAvailable { get => _IsAvailable; private set => _IsAvailable = value; }

        [DataMember]
        private bool _IsAvailable;
    }
}
