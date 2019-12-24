using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm
{
    [DataContract]
    public class DisplayTextSocketCommand : SocketCommand
    {
        [DataMember]
        private string _DisplayText;

        public DisplayTextSocketCommand(string displayText)
        {
            _DisplayText = displayText;
            CommandType = CommandType.DisplayTextToConsole;
        }

        
        public string DisplayText { get => _DisplayText; set => _DisplayText = value; }
    }
}
