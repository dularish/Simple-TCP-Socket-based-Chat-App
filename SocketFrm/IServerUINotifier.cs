using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketFrm.ClientMessageTypes;

namespace SocketFrm
{
    public interface IServerUINotifier
    {
        ManualResetEvent ServerWantsShutdown { get; }

        void HandleDisplayTextToConsoleMessage(DisplayTextClientMessage displayTextClientMessage);
        void HandleRegisterIDInServerMessage(RegisterIdClientMessage registerIdClientMessage);
        void HandleTransmitToPeerMessage(TransmitToPeerClientMessage transmitToPeerClientMessage);
        void NotifyClientDisconnection(string clientDisconnectedID);
        void LogException(Exception exception, string exceptionMessage);
        void LogText(string logMessage);
        void HandleSignInServerMessage(SignInClientMessage signInClientMessage);
    }
}
