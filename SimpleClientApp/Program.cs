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
using System.Configuration;

namespace SimpleClientApp
{
    public class ChatClientProgram
    {
        private static bool _isRegistrationInProcess = false;
        private static ClientAppState _clientAppState;
        static void Main(string[] args)
        {
            ConsoleNotifier clientUINotifier = new ConsoleNotifier();
            Action<string> registrationService;
            Task serverConnectionTask = ConnectWithServer(clientUINotifier, out registrationService);
            clientUINotifier.RegistrationService = registrationService;
            clientUINotifier.RequestRegistrationIdFromUser();
            serverConnectionTask.Wait();
        }
        /// <summary>
        /// Task that can be waited till the termination of connection
        /// </summary>
        /// <param name="clientAppState"></param>
        public static Task ConnectWithServer(IClientUINotifier clientUINotifier, out Action<string> registrationService)
        {
            ClientAppState clientAppState = new ClientAppState();
            IPAddress serverIP = IPAddress.Parse(ConfigurationManager.AppSettings["ServerIP"]);
            int serverPort = Int32.Parse(ConfigurationManager.AppSettings["ServerPort"]);
            clientAppState.TCPClient = new TcpClient();
            clientAppState.TCPClient.Connect(new IPEndPoint(serverIP, serverPort));
            
            //writeSimpleStringToStream(tcpClient);
            //writeSimpleCommandToStream(tcpClient);
            Task readStreamTask = new Task(() => readStream(clientAppState, clientUINotifier));
            readStreamTask.Start();
            Task writeStreamTask = new Task((someClientAppState) => writeStream(someClientAppState as ClientAppState), clientAppState);
            writeStreamTask.Start();
            Task shutDownWaitTask = new Task(() => clientUINotifier.ClientWantsShutdown.WaitOne());
            shutDownWaitTask.Start();
            //Task.Run(() => RandomlyQueueClientMessages());
            //Task.Run(() => { AskForRegistration(clientUINotifier); });
            _clientAppState = clientAppState;
            registrationService = submitRegistrationRequest;
            return Task.WhenAny(new List<Task>() { readStreamTask, writeStreamTask, shutDownWaitTask })
                .ContinueWith(x => 
                    {
                        clientAppState.TCPClient.Client.Shutdown(SocketShutdown.Both);
                        clientAppState.TCPClient.Close();
                    });
            
        }

        private static void submitRegistrationRequest(string registrationId)
        {
            if (!_isRegistrationInProcess)
            {
                if(registrationId.Length > 4)
                {
                    _clientAppState?.ClientMessagesQueue.Enqueue(new RegisterIdClientMessage(registrationId));
                    _isRegistrationInProcess = true;
                }
                else
                {
                    throw new Exception("Registration Id is too short");
                }
            }
            else
            {
                throw new Exception("Registration request already submitted");
            }
        }

        private static void writeStream(ClientAppState clientAppState)
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

        private static void readStream(ClientAppState clientAppState, IClientUINotifier clientUINotifier)
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
                        
                        handleServerMessage(serverMessage, clientUINotifier, clientAppState);
                    }
                }
                catch(IOException ioEx)
                {
                    clientUINotifier.HandleDisplayTextServerMessage( new DisplayTextServerMessage("Server was forcibly closed during the read process.. :("));
                    clientUINotifier.HandleDisplayTextServerMessage(new DisplayTextServerMessage("Message : " + ioEx.Message));
                    break;
                }
                catch (InvalidOperationException iopEx)
                {
                    clientUINotifier.HandleDisplayTextServerMessage(new DisplayTextServerMessage("Write process : Server had forcibly close the connection before .. :("));
                    clientUINotifier.HandleDisplayTextServerMessage(new DisplayTextServerMessage("Message : " + iopEx.Message));
                    break;
                }
                catch (Exception ex)
                {
                    clientUINotifier.HandleDisplayTextServerMessage(new DisplayTextServerMessage(ex.GetType().ToString() + "\n" + ex.Message + "\n" + ex.StackTrace));
                    break;
                }
            }
        }

        private static void handleServerMessage(ServerMessage serverMessage, IClientUINotifier clientUINotifier, ClientAppState clientAppState)
        {
            switch (serverMessage.ServerMessageType)
            {
                case ServerMessageType.DisplayTextToConsole:
                    clientUINotifier.HandleDisplayTextServerMessage(serverMessage as DisplayTextServerMessage);
                    break;
                case ServerMessageType.RegisterIdResult:
                    try
                    {
                        IPeerMessageTransmitter peerMessageTransmitter = (serverMessage as RegisterIdResultServerMessage)?.Result ?? false ? clientAppState : null;
                        clientUINotifier.HandleRegisterIDResultServerMessage(serverMessage as RegisterIdResultServerMessage, peerMessageTransmitter);
                    }
                    finally
                    {
                        _isRegistrationInProcess = false;
                    }
                    break;
                case ServerMessageType.ClientAvailabilityNotification:
                    clientUINotifier.HandleClientAvailabilityNotificationServerMessage(serverMessage as ClientAvailabilityNotificationServerMessage);
                    break;
                case ServerMessageType.TransmitToPeerResult:
                    clientUINotifier.HandleTransmitToPeerResultServerMessage(serverMessage as TransmitToPeerResultServerMessage);
                    break;
                case ServerMessageType.TransmitToPeer:
                    clientUINotifier.HandleTransmitToPeerServerMessage(serverMessage as TransmitToPeerServerMessage);
                    break;
                default:
                    break;
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
