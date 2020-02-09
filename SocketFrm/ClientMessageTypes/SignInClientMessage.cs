using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm.ClientMessageTypes
{
    [Serializable]
    [DataContract]
    public class SignInClientMessage : ClientMessage
    {
        [DataMember]
        private string _EmailId;
        [DataMember]
        private string _Password;

        public SignInClientMessage(string userEmail, string password)
        {
            EmailId = userEmail;
            Password = password;
            ClientMessageType = ClientMessageType.SignIn;
        }

        public string EmailId { get => _EmailId; private set => _EmailId = value; }
        public string Password { get => _Password; private set => _Password = value; }
    }
}
