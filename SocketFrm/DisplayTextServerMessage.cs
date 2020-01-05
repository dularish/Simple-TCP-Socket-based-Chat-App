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
    public class DisplayTextServerMessage : ServerMessage
    {
        [DataMember]
        private string _DisplayText;

        public DisplayTextServerMessage(string displayText)
        {
            DisplayText = displayText;
            ServerMessageType = ServerMessageType.DisplayTextToConsole;
        }

        public string DisplayText { get => _DisplayText; private set => _DisplayText = value; }
    }
}
