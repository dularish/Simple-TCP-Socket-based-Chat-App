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
    public class DisplayTextClientMessage : ClientMessage
    {
        [DataMember]
        private string _DisplayText;

        public DisplayTextClientMessage(string displayText)
        {
            _DisplayText = displayText;
            ClientMessageType = ClientMessageType.DisplayTextToConsole;
        }

        
        public string DisplayText { get => _DisplayText; set => _DisplayText = value; }
    }
}
