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

        private static ConcurrentQueue<ServerMessage> _serverMessagesQueue = new ConcurrentQueue<ServerMessage>();
        private Timer _serverMessagesQueueTimer;

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        public Client(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            ClientDisconnected += Client_ClientDisconnected;
            try
            {
                Task readStreamTask = new Task((someTcpClientObj) => this.ReadStream(someTcpClientObj as TcpClient), tcpClient);
                readStreamTask.Start();
                Task writeStreamTask = new Task((someTcpClientObj) => this.WriteStream(someTcpClientObj as TcpClient), tcpClient);
                writeStreamTask.Start();
                Task.Run(() => this.RandomlyQueueServerMessages());
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

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ServerMessage clientMessage = new DisplayTextServerMessage("Server says Now the time is " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
            _serverMessagesQueue.Enqueue(clientMessage);
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
                        ClientMessage clientMessage = ClientMessage.Deserialize(new StreamReader(networkStream));

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
                    displayTextToConsole(clientMessage as DisplayTextClientMessage);
                    break;
                default:
                    break;
            }
        }

        private static void displayTextToConsole(DisplayTextClientMessage displayTextSocketCommand)
        {
            Console.WriteLine(displayTextSocketCommand.DisplayText);
        }
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
