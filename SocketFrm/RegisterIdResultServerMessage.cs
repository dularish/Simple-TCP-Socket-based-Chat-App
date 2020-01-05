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
    public class RegisterIdResultServerMessage : ServerMessage
    {
        [DataMember]
        private bool _Result;

        public RegisterIdResultServerMessage(bool result)
        {
            Result = result;
            ServerMessageType = ServerMessageType.RegisterIdResult;
        }

        public bool Result { get => _Result; private set => _Result = value; }
    }
}
