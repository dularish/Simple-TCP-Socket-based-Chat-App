using ClientDesktopUI.ViewModels;
using SimpleClientApp;
using SocketFrm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientDesktopUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IClientUINotifier
    {
        private string _ClientRegistrationId;
        private Action<string> _RegistrationService;
        private AutoResetEvent _ClientWantsShutdown = new AutoResetEvent(false);
        private IPeerMessageTransmitter _PeerMessageTransmitter;
        public PeersListViewModel PeersListViewModel = new PeersListViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = PeersListViewModel;
            try
            {
                Task serverConnectionTask = ChatClientProgram.ConnectWithServer(this, out _RegistrationService);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect with server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RequestRegistrationIdFromUser();
        }

        private void RequestRegistrationIdFromUser(string message = "")
        {
            Action<string> onTryingToRegister = new Action<string>((someString) =>
            {
                _ClientRegistrationId = someString;
                _RegistrationService?.Invoke(someString);
            });
            ClientRegistrationWindow clientRegistrationWindow = new ClientRegistrationWindow(onTryingToRegister, message);
            clientRegistrationWindow.Show();
        }

        public AutoResetEvent ClientWantsShutdown => _ClientWantsShutdown;

        public void HandleClientAvailabilityNotificationServerMessage(ClientAvailabilityNotificationServerMessage clientAvailabilityNotificationServerMessage)
        {
            Dispatcher.Invoke(() =>
            {
                if (clientAvailabilityNotificationServerMessage.IsAvailable)
                {
                    PeersListViewModel.MakeUserAvailable(clientAvailabilityNotificationServerMessage.ClientUniqueId);
                }
                else
                {
                    PeersListViewModel.MakeUserNonAvailable(clientAvailabilityNotificationServerMessage.ClientUniqueId);
                }
            });
        }

        public void HandleDisplayTextServerMessage(DisplayTextServerMessage displayTextServerMessage)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Server : " + displayTextServerMessage.DisplayText, "Alert", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            });
        }

        public void HandleRegisterIDResultServerMessage(RegisterIdResultServerMessage registerIdResultServerMessage, IPeerMessageTransmitter clientAppState)
        {
            Dispatcher.Invoke(() =>
            {
                _PeerMessageTransmitter = clientAppState;
                if (registerIdResultServerMessage.Result)
                {
                    MessageBox.Show("Registration successful", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    RequestRegistrationIdFromUser("Please try another unique Id");
                }
            });
        }

        public void HandleTransmitToPeerResultServerMessage(TransmitToPeerResultServerMessage transmitToPeerResultServerMessage)
        {
            //to be implemented
        }

        public void HandleTransmitToPeerServerMessage(TransmitToPeerServerMessage transmitToPeerServerMessage)
        {
            Dispatcher.Invoke(() =>
            {
                PeersListViewModel.AddMessage(transmitToPeerServerMessage.TransmitToPeerClientMessage.SenderClientId, transmitToPeerServerMessage.TransmitToPeerClientMessage.TextMessage, MessageFrom.Them);
            });
        }

        private void _SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_CurrentMessageTextBox.Text))
            {
                PeersListViewModel.AddMessage((_PeersList.SelectedItem as PeerDataViewModel)?.PeerName ?? string.Empty, _CurrentMessageTextBox.Text.Trim(), MessageFrom.Me);

                _PeerMessageTransmitter.QueueToTransmitPeerMessage(new TransmitToPeerClientMessage(_CurrentMessageTextBox.Text.Trim(), (_PeersList.SelectedItem as PeerDataViewModel)?.PeerName ?? string.Empty, _ClientRegistrationId , 1));
            }
            _CurrentMessageTextBox.Clear();
        }
    }
}
