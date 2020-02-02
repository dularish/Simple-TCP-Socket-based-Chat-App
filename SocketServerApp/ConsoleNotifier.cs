using SocketFrm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServerApp
{
    public class ConsoleNotifier : IServerUINotifier
    {
        private ManualResetEvent _serverWantsShutdown = new ManualResetEvent(false);

        public ManualResetEvent ServerWantsShutdown => _serverWantsShutdown;

        public void HandleDisplayTextToConsoleMessage(DisplayTextClientMessage displayTextClientMessage)
        {
            Console.WriteLine(displayTextClientMessage.DisplayText);
        }

        public void HandleRegisterIDInServerMessage(RegisterIdClientMessage registerIdClientMessage)
        {
            Console.WriteLine("User registered. " + registerIdClientMessage.ID);
        }

        public void HandleTransmitToPeerMessage(TransmitToPeerClientMessage transmitToPeerClientMessage)
        {
            Console.WriteLine("Message transmitted from client " + transmitToPeerClientMessage.SenderMessageId + " to " + transmitToPeerClientMessage.ReceiverClientId);
        }

        public void LogException(Exception exception, string v)
        {
            Console.WriteLine("Exception!! : " + v);
            Console.WriteLine("Exception type : " + exception.GetType().FullName);
            Console.WriteLine("Exception message : " + exception.Message);
            Console.WriteLine("Exception stacktrace : " + exception.StackTrace);
        }

        public void LogText(string logMessage)
        {
            Console.WriteLine(logMessage);
        }

        public void NotifyClientDisconnection(string clientDisconnectedID)
        {
            Console.WriteLine("Client disconnected : " + clientDisconnectedID);
        }
    }
}
