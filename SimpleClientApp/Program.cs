using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SocketFrm;
using System.Collections.Concurrent;
using System.Timers;

namespace SimpleClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientAppState clientAppState = new ClientAppState(new ConsoleNotifier());
            ConnectWithServer(clientAppState);

        }
        /// <summary>
        /// Blocking call to connect with server
        /// </summary>
        /// <param name="clientAppState"></param>
        public static void ConnectWithServer(ClientAppState clientAppState)
        {
            IPAddress serverIP = IPAddress.Parse(System.Configuration.ConfigurationSettings.AppSettings["ServerIP"]);
            int serverPort = Int32.Parse(System.Configuration.ConfigurationSettings.AppSettings["ServerPort"]);
            clientAppState.TCPClient = new TcpClient();
            clientAppState.TCPClient.Connect(new IPEndPoint(serverIP, serverPort));
            try
            {
                //writeSimpleStringToStream(tcpClient);
                //writeSimpleCommandToStream(tcpClient);
                Task readStreamTask = new Task((someClientAppState) => ReadStream(someClientAppState as ClientAppState), clientAppState);
                readStreamTask.Start();
                Task writeStreamTask = new Task((someClientAppState) => WriteStream(someClientAppState as ClientAppState), clientAppState);
                writeStreamTask.Start();
                //Task.Run(() => RandomlyQueueClientMessages());
                Task.Run(() => { AskForRegistration(clientAppState); ContinuouslyGetInputFromUser(clientAppState); });

                Task.WhenAny(new List<Task>() { readStreamTask, writeStreamTask }).Wait();
            }
            finally
            {
                clientAppState.TCPClient.Client.Shutdown(SocketShutdown.Both);
                clientAppState.TCPClient.Close();
            }
        }

        private static void AskForRegistration(ClientAppState clientAppState)
        {
            while (true)
            {
                Console.WriteLine("Enter your desired registration Id :");
                string registrationId = Console.ReadLine();
                if ((registrationId?.Length ?? 0) > 5)
                {
                    clientAppState.ClientMessagesQueue.Enqueue(new RegisterIdClientMessage(registrationId));
                    clientAppState.ClientId = registrationId;//have to avoid this later
                    break;
                }
                else
                {
                    Console.WriteLine("Enter a valid registration Id.");
                }
            }
        }

        private static void ContinuouslyGetInputFromUser(ClientAppState clientAppState)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. See available registered clients");
                Console.WriteLine("2. Send text message");
                Console.WriteLine("Enter the choice : ");
                ConsoleKeyInfo choice = Console.ReadKey();

                switch (choice.KeyChar)
                {
                    case '1':
                        if(clientAppState.AvailableUsers.Count > 0)
                        {
                            Console.WriteLine(clientAppState.AvailableUsers.Aggregate((x, y) => x + "\n" + y));
                        }
                        else
                        {
                            Console.WriteLine(string.Empty);
                        }
                        break;
                    case '2':
                        if (string.IsNullOrEmpty(clientAppState.ClientId))
                        {
                            Console.WriteLine("Please get the user registerred before proceeding");
                        }
                        while (true)
                        {
                            Console.WriteLine("Enter the receiver clientId : (Enter \"!!Back\" to go one step back)");
                            string receiverClientId = Console.ReadLine();
                            if(receiverClientId == "!!Back")
                            {
                                break;
                            }
                            if (clientAppState.AvailableUsers.Contains(receiverClientId))
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
                                        clientAppState.ClientMessagesQueue.Enqueue(new TransmitToPeerClientMessage(message, receiverClientId, clientAppState.ClientId, 1));
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

        private static void WriteStream(ClientAppState clientAppState)
        {
            var tcpClient = clientAppState.TCPClient;
            var clientMessagesQueue = clientAppState.ClientMessagesQueue;
            while (true)
            {
                if (!clientMessagesQueue.IsEmpty)
                {
                    if(clientMessagesQueue.TryDequeue(out ClientMessage clientMessage))
                    {
                        try
                        {
                            NetworkStream networkStream = tcpClient.GetStream();

                            byte[] bufferData = clientMessage.Serialize(out int dataLength);

                            networkStream.Write(bufferData, 0, dataLength);
                            networkStream.Flush();

                        }
                        catch (IOException ioEx)
                        {
                            Console.WriteLine("Server was forcibly closed during the write process.. :(");
                            Console.WriteLine("Message : " + ioEx.Message);
                            break;
                        }
                        catch(InvalidOperationException iopEx)
                        {
                            Console.WriteLine("Read process : Server had forcibly close the connection before .. :(");
                            Console.WriteLine("Message : " + iopEx.Message);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.GetType().ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
                            
                            break;
                        }
                    }
                }
            }
            Console.ReadKey();
        }

        private static void ReadStream(ClientAppState clientAppState)
        {
            var tcpClient = clientAppState.TCPClient;
            NetworkStream networkStream = tcpClient.GetStream();

            while (true)
            {
                try
                {
                    if (networkStream.DataAvailable)
                    {
                        ServerMessage serverMessage = ServerMessage.Deserialize(networkStream);

                        handleServerMessage(serverMessage, clientAppState);
                    }
                }
                catch(IOException ioEx)
                {
                    Console.WriteLine("Server was forcibly closed during the read process.. :(");
                    Console.WriteLine("Message : " + ioEx.Message);
                    break;
                }
                catch (InvalidOperationException iopEx)
                {
                    Console.WriteLine("Write process : Server had forcibly close the connection before .. :(");
                    Console.WriteLine("Message : " + iopEx.Message);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType().ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
                    break;
                }
            }
            Console.ReadKey();
        }

        private static void handleServerMessage(ServerMessage serverMessage, ClientAppState clientAppState)
        {
            switch (serverMessage.ServerMessageType)
            {
                case ServerMessageType.DisplayTextToConsole:
                    clientAppState.ClientUINotifier.HandleDisplayTextServerMessage(serverMessage as DisplayTextServerMessage);
                    break;
                case ServerMessageType.RegisterIdResult:
                    clientAppState.ClientUINotifier.HandleRegisterIDResultServerMessage(serverMessage as RegisterIdResultServerMessage);
                    handleRegisterIDResultServerMessage(serverMessage as RegisterIdResultServerMessage, clientAppState);
                    break;
                case ServerMessageType.ClientAvailabilityNotification:
                    clientAppState.ClientUINotifier.HandleClientAvailabilityNotificationServerMessage(serverMessage as ClientAvailabilityNotificationServerMessage);
                    handleClientAvailabilityNotificationServerMessage(serverMessage as ClientAvailabilityNotificationServerMessage, clientAppState);
                    break;
                case ServerMessageType.TransmitToPeerResult:
                    clientAppState.ClientUINotifier.HandleTransmitToPeerResultServerMessage(serverMessage as TransmitToPeerResultServerMessage);
                    break;
                case ServerMessageType.TransmitToPeer:
                    clientAppState.ClientUINotifier.HandleTransmitToPeeServerMessage(serverMessage as TransmitToPeerServerMessage);
                    break;
                default:
                    break;
            }
        }

        private static void handleRegisterIDResultServerMessage(RegisterIdResultServerMessage registerIdResultServerMessage, ClientAppState clientAppState)
        {
            if (!registerIdResultServerMessage.Result)
            {
                clientAppState.ClientId = string.Empty;
            }
        }

        private static void handleClientAvailabilityNotificationServerMessage(ClientAvailabilityNotificationServerMessage clientAvailabilityNotificationServerMessage, ClientAppState clientAppState)
        {
            if (clientAvailabilityNotificationServerMessage.IsAvailable)
            {
                clientAppState.AvailableUsers.Add(clientAvailabilityNotificationServerMessage.ClientUniqueId);
            }
            else
            {
                clientAppState.AvailableUsers.Remove(clientAvailabilityNotificationServerMessage.ClientUniqueId);
            }
        }

        private static void writeSimpleCommandToStream(TcpClient tcpClient)
        {
            DisplayTextClientMessage displayTextSocketCommand = new DisplayTextClientMessage("Some simple string");

            NetworkStream networkStream = tcpClient.GetStream();

            byte[] dataBuffer = displayTextSocketCommand.Serialize(out int dataLength);

            //byte[] writeBuffer = new byte[sizeof(int)];
            //writeBuffer = BitConverter.GetBytes((int)dataLength);
            //networkStream.Write(writeBuffer, 0, sizeof(int));
            //networkStream.Flush();

            networkStream.Write(dataBuffer, 0, dataLength);
            networkStream.Flush();

            networkStream.Close();
        }

        private static void writeSimpleStringToStream(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            StreamWriter streamWriter = new StreamWriter(networkStream) { AutoFlush = true };
            streamWriter.Write("Hello from client");
            networkStream.Close();
        }

        private static string getLocalIPAddress()
        {
            string domainName = Dns.GetHostName();

            IPAddress iPAddress = null;

            foreach (var addressItem in Dns.GetHostEntry(domainName).AddressList)
            {
                Console.WriteLine("{0}/{1}", domainName, addressItem);
                if (addressItem.AddressFamily == AddressFamily.InterNetwork)
                {
                    iPAddress = addressItem;
                    break;
                }
            }

            return iPAddress.ToString();
        }
    }
}
