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
        private List<string> _availableUsers = new List<string>();
        private string _clientId = "Unnamed";

        public List<string> AvailableUsers { get => _availableUsers; set => _availableUsers = value; }
        public string ClientId { get => _clientId; set => _clientId = value; }

        public void HandleDisplayTextServerMessage(DisplayTextServerMessage displayTextServerMessage)
        {
            Console.WriteLine(displayTextServerMessage.DisplayText);
        }

        public void HandleRegisterIDResultServerMessage(RegisterIdResultServerMessage registerIdResultServerMessage, IPeerMessageTransmitter peerMessageTransmitter)
        {
            if (registerIdResultServerMessage.Result)
            {
                Console.WriteLine("Registration successful!! ");
                Task.Run(() => { startListeningToUI(peerMessageTransmitter); });
            }
            else
            {
                Console.WriteLine("Registration failed. Please try another ID");

                ClientId = string.Empty;
            }
        }

        public void HandleClientAvailabilityNotificationServerMessage(ClientAvailabilityNotificationServerMessage clientAvailabilityNotificationServerMessage)
        {
            string textForStatus = clientAvailabilityNotificationServerMessage.IsAvailable ? " has joined" : " has left";
            Console.WriteLine(clientAvailabilityNotificationServerMessage.ClientUniqueId + textForStatus);

            if (clientAvailabilityNotificationServerMessage.IsAvailable)
            {
                AvailableUsers.Add(clientAvailabilityNotificationServerMessage.ClientUniqueId);
            }
            else
            {
                AvailableUsers.Remove(clientAvailabilityNotificationServerMessage.ClientUniqueId);
            }
        }

        public void HandleTransmitToPeerResultServerMessage(TransmitToPeerResultServerMessage transmitToPeerResultServerMessage)
        {
            //doing nothing
        }

        public void HandleTransmitToPeeServerMessage(TransmitToPeerServerMessage transmitToPeerServerMessage)
        {
            Console.WriteLine(transmitToPeerServerMessage.TransmitToPeerClientMessage.SenderClientId + " : " + transmitToPeerServerMessage.TransmitToPeerClientMessage.TextMessage);
        }

        public string GetRegistrationId(string validationErrorMessage)
        {
            if (!string.IsNullOrEmpty(validationErrorMessage))
            {
                Console.WriteLine(validationErrorMessage);
            }
            Console.WriteLine("Enter the loginId that you would like to identify yourself with");
            string registrationId = Console.ReadLine();
            ClientId = registrationId;
            return registrationId;
        }

        private void startListeningToUI(IPeerMessageTransmitter peerMessageTransmitter)
        {
            while (true)
            {
                Console.WriteLine("1. See available registered clients");
                Console.WriteLine("2. Send text message");
                Console.WriteLine("Enter the choice : ");
                ConsoleKeyInfo choice = Console.ReadKey();

                switch (choice.KeyChar)
                {
                    case '1':
                        if (AvailableUsers.Count > 0)
                        {
                            Console.WriteLine(AvailableUsers.Aggregate((x, y) => x + "\n" + y));
                        }
                        else
                        {
                            Console.WriteLine(string.Empty);
                        }
                        break;
                    case '2':
                        if (string.IsNullOrEmpty(ClientId))
                        {
                            Console.WriteLine("Please get the user registerred before proceeding");
                        }
                        while (true)
                        {
                            Console.WriteLine("Enter the receiver clientId : (Enter \"!!Back\" to go one step back)");
                            string receiverClientId = Console.ReadLine();
                            if (receiverClientId == "!!Back")
                            {
                                break;
                            }
                            if (AvailableUsers.Contains(receiverClientId))
                            {
                                Console.WriteLine("Continue entering the messages (Enter \"!!Back\" to go one step back)");
                                while (true)
                                {
                                    string message = Console.ReadLine();
                                    if (message == "!!Back")
                                    {
                                        break;
                                    }
                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        peerMessageTransmitter.QueueToTransmitPeerMessage(new TransmitToPeerClientMessage(message, receiverClientId, ClientId, 1));
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
