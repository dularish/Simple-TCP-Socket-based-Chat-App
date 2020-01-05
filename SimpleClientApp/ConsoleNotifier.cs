using SocketFrm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClientApp
{
    public class ConsoleNotifier : IClientUINotifier
    {
        public void HandleDisplayTextServerMessage(DisplayTextServerMessage displayTextServerMessage)
        {
            Console.WriteLine(displayTextServerMessage.DisplayText);
        }

        public void HandleRegisterIDResultServerMessage(RegisterIdResultServerMessage registerIdResultServerMessage)
        {
            if (registerIdResultServerMessage.Result)
            {
                Console.WriteLine("Registration successful!! ");
            }
            else
            {
                Console.WriteLine("Registration failed. Please try another ID");
            }
        }

        public void HandleClientAvailabilityNotificationServerMessage(ClientAvailabilityNotificationServerMessage clientAvailabilityNotificationServerMessage)
        {
            string textForStatus = clientAvailabilityNotificationServerMessage.IsAvailable ? " has joined" : " has left";
            Console.WriteLine(clientAvailabilityNotificationServerMessage.ClientUniqueId + textForStatus);
        }

        public void HandleTransmitToPeerResultServerMessage(TransmitToPeerResultServerMessage transmitToPeerResultServerMessage)
        {
            //doing nothing
        }

        public void HandleTransmitToPeeServerMessage(TransmitToPeerServerMessage transmitToPeerServerMessage)
        {
            Console.WriteLine(transmitToPeerServerMessage.TransmitToPeerClientMessage.SenderClientId + " : " + transmitToPeerServerMessage.TransmitToPeerClientMessage.TextMessage);
        }
    }
}
