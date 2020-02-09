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
        private string _emailId;

        [DataMember]
        private string _password;

        public RegisterIdClientMessage(string emailId, string password)
        {
            EmailId = emailId;
            Password = password;
            ClientMessageType = ClientMessageType.RegisterIDInServer;
        }

        public string EmailId { get => _emailId; private set => _emailId = value; }
        public string Password { get => _password; private set => _password = value; }
    }
}
