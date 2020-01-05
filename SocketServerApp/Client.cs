using SocketFrm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SocketServerApp
{
    public class Client
    {
        private TcpClient tcpClient;
        private IServerUINotifier _UINotifier;
        private ConcurrentQueue<ServerMessage> _serverMessagesQueue = new ConcurrentQueue<ServerMessage>();
        private Timer _serverMessagesQueueTimer;
        private string _ID;

        public string ID { get => _ID; private set => _ID = value; }

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        public event EventHandler<ClientRegisterRequestEventArgs> ClientRegisterRequested;
        public event EventHandler<ClientTransmittedPeerMessageEventArgs> ClientTransmittedPeerMessage;

        public Client(TcpClient tcpClient, IServerUINotifier uiNotifier)
        {
            this.tcpClient = tcpClient;
            _UINotifier = uiNotifier;
            ClientDisconnected += Client_ClientDisconnected;
            try
            {
                Task readStreamTask = new Task((someTcpClientObj) => this.ReadStream(someTcpClientObj as TcpClient), tcpClient);
                readStreamTask.Start();
                Task writeStreamTask = new Task((someTcpClientObj) => this.WriteStream(someTcpClientObj as TcpClient), tcpClient);
                writeStreamTask.Start();
                //Task.Run(() => this.RandomlyQueueServerMessages());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Closes the running tasks on disconnection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _serverMessagesQueueTimer.Stop();
            _serverMessagesQueueTimer.Dispose();
        }

        private void RandomlyQueueServerMessages()
        {
            Random randGen = new Random(36);
            if(_serverMessagesQueueTimer != null)
            {
                _serverMessagesQueueTimer.Stop();
                _serverMessagesQueueTimer.Dispose();
            }
            _serverMessagesQueueTimer = new Timer(randGen.Next(3000, 6000));
            _serverMessagesQueueTimer.Elapsed += Timer_Elapsed;
            _serverMessagesQueueTimer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ServerMessage serverMessage = new DisplayTextServerMessage("Server says Now the time is " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
            _serverMessagesQueue.Enqueue(serverMessage);
        }

        public void EnqueueServerMessage(ServerMessage serverMessage)
        {
            _serverMessagesQueue.Enqueue(serverMessage);
        }

        private void WriteStream(TcpClient tcpClient)
        {
            while (true)
            {
                if (!_serverMessagesQueue.IsEmpty)
                {
                    if (_serverMessagesQueue.TryDequeue(out ServerMessage serverMessage))
                    {
                        try
                        {
                            NetworkStream networkStream = tcpClient.GetStream();

                            byte[] bufferData = serverMessage.Serialize(out int dataLength);

                            networkStream.Write(bufferData, 0, dataLength);
                            networkStream.Flush();

                        }
                        catch (IOException ioEx)
                        {
                            Console.WriteLine("The client was forcibly closed during the write process.. :(");
                            Console.WriteLine("Message : " + ioEx.Message + "\nStackTrace : " + ioEx.StackTrace + "\n\n");
                            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
                            break;
                        }
                        catch (InvalidOperationException iopEx)
                        {
                            Console.WriteLine("Write process : Client had forcibly closed the connection before .. :(");
                            Console.WriteLine("Message : " + iopEx.Message);
                            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
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

        private void ReadStream(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();

            while (true)
            {
                try
                {
                    if (networkStream.DataAvailable)
                    {
                        ClientMessage clientMessage = ClientMessage.Deserialize(networkStream);

                        handleClientMessage(clientMessage);
                    }
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine("The client was forcibly closed during the read process.. :(");
                    Console.WriteLine("Message : " + ioEx.Message);
                    ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
                    break;
                }
                catch (InvalidOperationException iopEx)
                {
                    Console.WriteLine("Read process : Client had forcibly closed the connection before .. :(");
                    Console.WriteLine("Message : " + iopEx.Message);
                    ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
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

        private void handleClientMessage(ClientMessage clientMessage)
        {
            switch (clientMessage.ClientMessageType)
            {
                case ClientMessageType.DisplayTextToConsole:
                    _UINotifier.HandleDisplayTextToConsoleMessage(clientMessage as DisplayTextClientMessage);
                    break;
                case ClientMessageType.RegisterIDInServer:
                    RegisterIdClientMessage registerIdClientMessage = clientMessage as RegisterIdClientMessage;
                    ID = registerIdClientMessage.ID;
                    ClientRegisterRequested?.Invoke(this, new ClientRegisterRequestEventArgs(registerIdClientMessage.ID, this));
                    _UINotifier.HandleRegisterIDInServerMessage(registerIdClientMessage);
                    break;
                case ClientMessageType.TransmitToPeer:
                    TransmitToPeerClientMessage transmitToPeerClientMessage = clientMessage as TransmitToPeerClientMessage;
                    ClientTransmittedPeerMessage?.Invoke(this, new ClientTransmittedPeerMessageEventArgs(transmitToPeerClientMessage));
                    _UINotifier.HandleTransmitToPeerMessage(transmitToPeerClientMessage);
                    break;
                default:
                    break;
            }
        }
    }

    public class ClientTransmittedPeerMessageEventArgs : EventArgs
    {
        private TransmitToPeerClientMessage _TransmitToPeerClientMessage;

        public ClientTransmittedPeerMessageEventArgs(TransmitToPeerClientMessage transmitToPeerClientMessage)
        {
            this.TransmitToPeerClientMessage = transmitToPeerClientMessage;
        }

        public TransmitToPeerClientMessage TransmitToPeerClientMessage { get => _TransmitToPeerClientMessage; private set => _TransmitToPeerClientMessage = value; }
    }

    public class ClientRegisterRequestEventArgs : EventArgs
    {
        private string _ID;
        private Client _Client;

        public ClientRegisterRequestEventArgs(string id, Client client)
        {
            ID = id;
            Client = client;
        }

        public string ID { get => _ID; private set => _ID = value; }
        public Client Client { get => _Client; private set => _Client = value; }
    }

    public class ClientDisconnectedEventArgs : EventArgs
    {
        private Client _clientDisconnected;

        public Client ClientDisconnected
        {
            get
            {
                return _clientDisconnected;
            }
            private set
            {
                _clientDisconnected = value;
            }
        }

        public ClientDisconnectedEventArgs(Client clientDisconnected)
        {
            ClientDisconnected = clientDisconnected;
        }
    }

}
