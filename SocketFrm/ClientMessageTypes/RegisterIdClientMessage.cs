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
    public class RegisterIdClientMessage : ClientMessage
    {
        [DataMember]
        private string _ID;

        public RegisterIdClientMessage(string id)
        {
            ID = id;
            ClientMessageType = ClientMessageType.RegisterIDInServer;
        }

        public string ID { get => _ID; private set => _ID = value; }
    }
}
