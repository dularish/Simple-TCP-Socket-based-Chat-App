using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketFrm
{
    public interface IServerUINotifier
    {
        ManualResetEvent ServerWantsShutdown { get; }

        void HandleDisplayTextToConsoleMessage(DisplayTextClientMessage displayTextClientMessage);
        void HandleRegisterIDInServerMessage(RegisterIdClientMessage registerIdClientMessage);
        void HandleTransmitToPeerMessage(TransmitToPeerClientMessage transmitToPeerClientMessage);
        void NotifyClientDisconnection(string clientDisconnectedID);
    }
}
