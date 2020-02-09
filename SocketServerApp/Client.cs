using SocketFrm;
using SocketFrm.ClientMessageTypes;
using SocketFrm.ServerMessageTypes;
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
        private Timer _keepAliveTimer;
        private object _writeProcessLockObj = new object();
        private string _ID;

        public string ID { get => _ID; private set => _ID = value; }

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        public event EventHandler<ClientRegisterRequestEventArgs> ClientRegisterRequested;
        public event EventHandler<ClientSignInRequestEventArgs> ClientSignInRequested;
        public event EventHandler<ClientTransmittedPeerMessageEventArgs> ClientTransmittedPeerMessage;

        public Client(TcpClient tcpClient, IServerUINotifier uiNotifier)
        {
            this.tcpClient = tcpClient;
            _UINotifier = uiNotifier;
            ClientDisconnected += Client_ClientDisconnected;
            initializeKeepAliveTimer();
            try
            {
                ReadConnectionState connectionState = new ReadConnectionState(tcpClient);
                tcpClient.Client.BeginReceive(connectionState.DataSizeBuffer, 0, connectionState.DataSizeBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), connectionState);
            }
            catch (Exception ex)
            {
                _UINotifier.LogException(ex, "unexpected exception during setting up the tcpClient");
            }
        }

        internal void ForceDisconnect()
        {
            this.tcpClient?.Close();
            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            ReadConnectionState connectionState = ar.AsyncState as ReadConnectionState;

            try
            {
                int bytesReceived = connectionState.TcpClient.Client.EndReceive(ar);

                if (bytesReceived != connectionState.DataSizeBuffer.Length)
                {
                    //Something went wrong, hence force disconnect the client
                    throw new Exception("Data size partially received");
                }
                else
                {
                    int dataSize = BitConverter.ToInt32(connectionState.DataSizeBuffer, 0);

                    if (dataSize <= 0)
                    {
                        //Something went wrong
                        throw new Exception("Data size overflow/incorrect");
                    }
                    else
                    {
                        byte[] data = new byte[dataSize];
                        connectionState.TcpClient.Client.Receive(data, 0, data.Length, SocketFlags.None);
                        ClientMessage clientMessage = ClientMessage.Deserialize(data);

                        handleClientMessage(clientMessage);//Look into possibilities to run it parallel away from this flow

                        tcpClient.Client.BeginReceive(connectionState.DataSizeBuffer, 0, connectionState.DataSizeBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), connectionState);
                    }
                }
            }
            catch(SocketException sockEx)
            {
                _UINotifier.LogException(sockEx as Exception, "Unexpected SocketException during receiving data");
                connectionState?.TcpClient?.Close();
                ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
            }
            catch (Exception ex)
            {
                _UINotifier.LogException(ex, "Unexpected exception during receiving data");
                connectionState?.TcpClient?.Close();
                ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
            }
        }

        private void initializeKeepAliveTimer()
        {
            if (_keepAliveTimer != null)
            {
                _keepAliveTimer.Stop();
                _keepAliveTimer.Dispose();
            }
            _keepAliveTimer = new Timer(3000);
            _keepAliveTimer.Elapsed += _keepAliveTimer_Elapsed; ;
            _keepAliveTimer.Start();
        }

        private void _keepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ServerMessage serverMessage = new KeepAliveServerMessage();
            EnqueueServerMessage(serverMessage);
        }

        /// <summary>
        /// Closes the running tasks on disconnection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _keepAliveTimer?.Stop();
            _keepAliveTimer?.Dispose();
        }

        public void EnqueueServerMessage(ServerMessage serverMessage)
        {
            lock (_writeProcessLockObj)
            {
                byte[] bufferData = serverMessage.Serialize(out int dataLength);
                WriteConnectionState writeConnectionState = new WriteConnectionState(tcpClient, bufferData.Length);
                tcpClient.Client.BeginSend(bufferData,0, bufferData.Length, SocketFlags.None, new AsyncCallback(SendCallback), writeConnectionState);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            WriteConnectionState connectionState = ar.AsyncState as WriteConnectionState;

            try
            {
                int bytesSent = connectionState.TcpClient.Client.EndSend(ar, out SocketError socketError);

                if(socketError != SocketError.Success)
                {
                    throw new Exception(socketError.ToString());
                }

                if(bytesSent != connectionState.SentDataLength)
                {
                    throw new Exception("Sent data doesn't match with the data length to be sent");
                }
            }
            catch (SocketException sockEx)
            {
                _UINotifier.LogException(sockEx as Exception, "Unexpected SocketException during sending data");
                connectionState?.TcpClient?.Close();
                ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
            }
            catch (Exception ex)
            {
                _UINotifier.LogException(ex, "Unexpected exception during sending data");
                connectionState?.TcpClient?.Close();
                ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this));
            }
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
                    ID = registerIdClientMessage.EmailId;
                    ClientRegisterRequested?.Invoke(this, new ClientRegisterRequestEventArgs(registerIdClientMessage.EmailId, registerIdClientMessage.Password, this));
                    _UINotifier.HandleRegisterIDInServerMessage(registerIdClientMessage);
                    break;
                case ClientMessageType.SignIn:
                    SignInClientMessage signInClientMessage = clientMessage as SignInClientMessage;
                    ID = signInClientMessage.EmailId;
                    ClientSignInRequested?.Invoke(this, new ClientSignInRequestEventArgs(signInClientMessage.EmailId, signInClientMessage.Password, this));
                    _UINotifier.HandleSignInServerMessage(signInClientMessage);
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
        private readonly string _userEmail;
        private readonly string _password;
        private Client _Client;

        public ClientRegisterRequestEventArgs(string userEmail, string password, Client client)
        {
            this._userEmail = userEmail;
            this._password = password;
            Client = client;
        }

        public Client Client { get => _Client; private set => _Client = value; }

        public string UserEmail => _userEmail;

        public string Password => _password;
    }

    public class ClientSignInRequestEventArgs : EventArgs
    {
        private readonly string _userEmail;
        private readonly string _password;
        private readonly Client _client;

        public ClientSignInRequestEventArgs(string userEmail, string password, Client client)
        {
            this._userEmail = userEmail;
            this._password = password;
            this._client = client;
        }

        public string UserEmail => _userEmail;

        public string Password => _password;

        public Client Client => _client;
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
