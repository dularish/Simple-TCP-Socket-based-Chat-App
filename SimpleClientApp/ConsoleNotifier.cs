using SocketFrm;
using SocketFrm.ServerMessageTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleClientApp
{
    public class ConsoleNotifier : IClientUINotifier
    {
        private List<string> _availableUsers = new List<string>();
        private string _clientId = "Unnamed";
        private ManualResetEvent _clientWantsShutdown = new ManualResetEvent(false);

        internal Action<string, string> RegistrationService { get; set; }

        internal Action<string, string> LoginService { get; set; }
        public List<string> AvailableUsers { get => _availableUsers; set => _availableUsers = value; }
        public string ClientId { get => _clientId; set => _clientId = value; }

        public ManualResetEvent ClientWantsShutdown => _clientWantsShutdown;

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
                RequestRegistrationIdFromUser();
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

        public void HandleTransmitToPeerServerMessage(TransmitToPeerServerMessage transmitToPeerServerMessage)
        {
            Console.WriteLine(transmitToPeerServerMessage.TransmitToPeerClientMessage.SenderClientId + " : " + transmitToPeerServerMessage.TransmitToPeerClientMessage.TextMessage);
        }

        private void startListeningToUI(IPeerMessageTransmitter peerMessageTransmitter)
        {
            while (true)
            {
                Console.WriteLine("1. See available registered clients");
                Console.WriteLine("2. Send text message");
                Console.WriteLine("3. Close the application");
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
                    case '3':
                        ClientWantsShutdown?.Set();
                        break;
                    default:
                        break;
                }
            }
        }

        public void RequestRegistrationIdFromUser()
        {
            Console.WriteLine("Enter your emailId");
            string registrationId = Console.ReadLine();
            Console.WriteLine("Enter the password");
            string password = Console.ReadLine();
            ClientId = registrationId;
            RegistrationService?.Invoke(registrationId, password);
        }

        public void HandleSignInResultServerMessage(SignInResultServerMessage signInResultServerMessage, IPeerMessageTransmitter peerMessageTransmitter)
        {
            if (signInResultServerMessage.Result)
            {
                Console.WriteLine("Login successful!! ");
                Task.Run(() => { startListeningToUI(peerMessageTransmitter); });
            }
            else
            {
                Console.WriteLine("Login failed. Please try another ID");

                ClientId = string.Empty;
                RequestRegistrationIdFromUser();
            }
        }
    }
}
