using SocketFrm;
using SocketFrm.ServerMessageTypes;
using SocketServerApp.Authentication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerApp
{
    public class ClientsManager
    {
        private List<Client> _ClientsConnected = new List<Client>();
        private IServerUINotifier _UINotifier;
        private readonly Authentication.IAuthenticationService _authenticationService;
        private ConcurrentDictionary<string, Client> _RegisteredClients = new ConcurrentDictionary<string, Client>();

        public ClientsManager(IServerUINotifier uiNotifier, Authentication.IAuthenticationService authenticationService)
        {
            _UINotifier = uiNotifier;
            _authenticationService = authenticationService;
        }

        public void AcceptClient(TcpClient tcpClient)
        {
            Client newClient = new Client(tcpClient, _UINotifier);
            _ClientsConnected.Add(newClient);
            newClient.ClientDisconnected += OnClientDisconnected;
            newClient.ClientRegisterRequested += OnClientRegisterRequested;
            newClient.ClientSignInRequested += OnClientSignInRequested;
            newClient.ClientTransmittedPeerMessage += OnClientTransmittedPeerMessage;
        }

        private void OnClientSignInRequested(object sender, ClientSignInRequestEventArgs e)
        {
            if (_RegisteredClients.ContainsKey(e.UserEmail))
            {
                _RegisteredClients[e.UserEmail].ForceDisconnect();
            }

            if (_RegisteredClients.ContainsKey(e.UserEmail))
            {
                e.Client.EnqueueServerMessage(new SignInResultServerMessage(false));
                return;
            }

            bool signInResult = _authenticationService.SignIn(e.UserEmail, e.Password, out SignInErrorCode signInErrorCode);

            if (signInResult)
            {
                _RegisteredClients[e.UserEmail] = e.Client;
                e.Client.EnqueueServerMessage(new SignInResultServerMessage(true));
                _ClientsConnected.ForEach((client) =>
                {
                    if (client != e.Client)
                    {
                        client.EnqueueServerMessage(new ClientAvailabilityNotificationServerMessage(e.UserEmail, true));
                        e.Client.EnqueueServerMessage(new ClientAvailabilityNotificationServerMessage(client.ID, true));
                    }
                });
            }
            else
            {
                e.Client.EnqueueServerMessage(new SignInResultServerMessage(false));
            }
        }

        private void OnClientTransmittedPeerMessage(object sender, ClientTransmittedPeerMessageEventArgs e)
        {
            if (_RegisteredClients.ContainsKey(e.TransmitToPeerClientMessage.ReceiverClientId))
            {
                _RegisteredClients[e.TransmitToPeerClientMessage.ReceiverClientId].EnqueueServerMessage(new TransmitToPeerServerMessage(e.TransmitToPeerClientMessage));
                //Raise server message stating the peer message transmit success to the sender
                _RegisteredClients[e.TransmitToPeerClientMessage.SenderClientId].EnqueueServerMessage(new TransmitToPeerResultServerMessage(e.TransmitToPeerClientMessage.ReceiverClientId, e.TransmitToPeerClientMessage.SenderMessageId, true));
            }
            else
            {
                //Raise server message stating the peer message transmit failed to the sender
                _RegisteredClients[e.TransmitToPeerClientMessage.SenderClientId].EnqueueServerMessage(new TransmitToPeerResultServerMessage(e.TransmitToPeerClientMessage.ReceiverClientId, e.TransmitToPeerClientMessage.SenderMessageId, false));
            }
        }

        private void OnClientRegisterRequested(object sender, ClientRegisterRequestEventArgs e)
        {
            bool signUpResult = _authenticationService.SignUp(e.UserEmail, e.Password, out SignUpErrorCode signUpErrorCode);

            if (signUpResult)
            {
                _RegisteredClients[e.UserEmail] = e.Client;
                e.Client.EnqueueServerMessage(new RegisterIdResultServerMessage(true));
                _ClientsConnected.ForEach((client) =>
                {
                    if (client != e.Client)
                    {
                        client.EnqueueServerMessage(new ClientAvailabilityNotificationServerMessage(e.UserEmail, true));
                        e.Client.EnqueueServerMessage(new ClientAvailabilityNotificationServerMessage(client.ID, true));
                    }
                });
            }
            else
            {
                e.Client.EnqueueServerMessage(new RegisterIdResultServerMessage(false));
            }
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _ClientsConnected.Remove(e.ClientDisconnected);
            if(e?.ClientDisconnected?.ID != null)
            {
                _RegisteredClients.TryRemove(e.ClientDisconnected.ID, out Client removedClient);
            }
            _ClientsConnected.ForEach((client) =>
            {
                if (client != e.ClientDisconnected)
                {
                    client.EnqueueServerMessage(new ClientAvailabilityNotificationServerMessage(e.ClientDisconnected.ID, false));
                }
            });

            _UINotifier.NotifyClientDisconnection(e.ClientDisconnected.ID);
            
        }
    }
}
