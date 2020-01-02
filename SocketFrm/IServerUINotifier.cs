using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm
{
    public interface IServerUINotifier
    {
        void HandleDisplayTextToConsoleMessage(DisplayTextClientMessage displayTextClientMessage);
        void HandleRegisterIDInServerMessage(RegisterIdClientMessage registerIdClientMessage);
        void HandleTransmitToPeerMessage(TransmitToPeerClientMessage transmitToPeerClientMessage);
        void NotifyClientDisconnection(string clientDisconnectedID);
    }
}
