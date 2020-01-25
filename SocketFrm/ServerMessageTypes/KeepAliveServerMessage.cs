using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm.ServerMessageTypes
{
    [Serializable]
    [DataContract]
    public class KeepAliveServerMessage : ServerMessage
    {
        public KeepAliveServerMessage()
        {
            ServerMessageType = ServerMessageType.KeepAlive;
        }
    }
}
