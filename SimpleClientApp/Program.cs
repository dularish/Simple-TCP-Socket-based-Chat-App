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
    class Program
    {
        static void Main(string[] args)
        {
            ClientAppState clientAppState = new ClientAppState();
            IClientUINotifier clientUINotifier = new ConsoleNotifier();
            ConnectWithServer(clientAppState, clientUINotifier);

        }
        /// <summary>
        /// Blocking call to connect with server
        /// </summary>
        /// <param name="clientAppState"></param>
        public static void ConnectWithServer(ClientAppState clientAppState, IClientUINotifier clientUINotifier)
        {
            IPAddress serverIP = IPAddress.Parse(ConfigurationManager.AppSettings["ServerIP"]);
            int serverPort = Int32.Parse(ConfigurationManager.AppSettings["ServerPort"]);
            clientAppState.TCPClient = new TcpClient();
            clientAppState.TCPClient.Connect(new IPEndPoint(serverIP, serverPort));
            try
            {
                //writeSimpleStringToStream(tcpClient);
                //writeSimpleCommandToStream(tcpClient);
                Task readStreamTask = new Task(() => ReadStream(clientAppState, clientUINotifier));
                readStreamTask.Start();
                Task writeStreamTask = new Task((someClientAppState) => WriteStream(someClientAppState as ClientAppState), clientAppState);
                writeStreamTask.Start();
                //Task.Run(() => RandomlyQueueClientMessages());
                Task.Run(() => { AskForRegistration(clientAppState, clientUINotifier); });

                Task.WhenAny(new List<Task>() { readStreamTask, writeStreamTask }).Wait();
            }
            finally
            {
                clientAppState.TCPClient.Client.Shutdown(SocketShutdown.Both);
                clientAppState.TCPClient.Close();
            }
        }

        private static void AskForRegistration(ClientAppState clientAppState, IClientUINotifier clientUINotifier)
        {
            string validationErrorMessage = string.Empty;
            while (true)
            {
                string registrationId = clientUINotifier.GetRegistrationId(validationErrorMessage);
                if ((registrationId?.Length ?? 0) > 5)
                {
                    clientAppState.ClientMessagesQueue.Enqueue(new RegisterIdClientMessage(registrationId));
                    break;
                }
                else
                {
                    validationErrorMessage = "Enter a valid registration Id.";
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

        private static void ReadStream(ClientAppState clientAppState, IClientUINotifier clientUINotifier)
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

        private static void handleServerMessage(ServerMessage serverMessage, IClientUINotifier clientUINotifier, IPeerMessageTransmitter clientAppState)
        {
            switch (serverMessage.ServerMessageType)
            {
                case ServerMessageType.DisplayTextToConsole:
                    clientUINotifier.HandleDisplayTextServerMessage(serverMessage as DisplayTextServerMessage);
                    break;
                case ServerMessageType.RegisterIdResult:
                    IPeerMessageTransmitter peerMessageTransmitter = (serverMessage as RegisterIdResultServerMessage)?.Result ?? false ? clientAppState : null;
                    clientUINotifier.HandleRegisterIDResultServerMessage(serverMessage as RegisterIdResultServerMessage, peerMessageTransmitter);
                    break;
                case ServerMessageType.ClientAvailabilityNotification:
                    clientUINotifier.HandleClientAvailabilityNotificationServerMessage(serverMessage as ClientAvailabilityNotificationServerMessage);
                    break;
                case ServerMessageType.TransmitToPeerResult:
                    clientUINotifier.HandleTransmitToPeerResultServerMessage(serverMessage as TransmitToPeerResultServerMessage);
                    break;
                case ServerMessageType.TransmitToPeer:
                    clientUINotifier.HandleTransmitToPeeServerMessage(serverMessage as TransmitToPeerServerMessage);
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
