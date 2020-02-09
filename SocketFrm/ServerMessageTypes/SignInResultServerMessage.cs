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
    public class SignInResultServerMessage : ServerMessage
    {
        [DataMember]
        private bool _Result;

        public SignInResultServerMessage(bool result)
        {
            Result = result;
            ServerMessageType = ServerMessageType.SignInResult;
        }

        public bool Result { get => _Result; private set => _Result = value; }
    }
}
