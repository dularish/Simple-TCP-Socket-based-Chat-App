using ClientDesktopUI.ViewModels;
using SimpleClientApp;
using SocketFrm;
using SocketFrm.ServerMessageTypes;
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
        private Action<string, string> _RegistrationService;
        private Action<string, string> _LoginService;
        private ManualResetEvent _ClientWantsShutdown = new ManualResetEvent(false);
        private IPeerMessageTransmitter _PeerMessageTransmitter;
        public PeersListViewModel PeersListViewModel = new PeersListViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = PeersListViewModel;
            try
            {
                Task serverConnectionTask = ChatClientProgram.ConnectWithServer(this, out _RegistrationService, out _LoginService);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect with server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClientWantsShutdown.Set();
            Properties.Settings.Default.Save();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RequestRegistrationIdFromUser();
        }

        private void RequestRegistrationIdFromUser(string message = "")
        {
            Action<string, string> onTryingToRegister = new Action<string, string>((emailId, password) =>
            {
                _ClientRegistrationId = emailId;
                _RegistrationService?.Invoke(emailId, password);
            });
            Action<string, string> onTryingToLogin = new Action<string, string>((emailId, password) =>
            {
                _ClientRegistrationId = emailId;
                _LoginService?.Invoke(emailId, password);
            });
            ClientRegistrationWindow clientRegistrationWindow = new ClientRegistrationWindow(onTryingToRegister, onTryingToLogin, message);
            clientRegistrationWindow.Show();
        }

        public ManualResetEvent ClientWantsShutdown => _ClientWantsShutdown;

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

                if(_PeersList.SelectedIndex == -1 && _PeersList.Items.Count > 0)
                {
                    _PeersList.SelectedIndex = 0;
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

                alertUserIfNeeded();
            });
        }

        private void alertUserIfNeeded()
        {
            if (!WindowsHelper.IsApplicationActive())
            {
                System.Media.SystemSounds.Beep.Play();
                WindowsHelper.FlashWindow(this);
            }
        }

        private void _SendBtn_Click(object sender, RoutedEventArgs e)
        {
            sendMessage();
        }

        private void sendMessage()
        {
            if (!string.IsNullOrWhiteSpace(_CurrentMessageTextBox.Text))
            {
                PeersListViewModel.AddMessage((_PeersList.SelectedItem as PeerDataViewModel)?.PeerName ?? string.Empty, _CurrentMessageTextBox.Text.Trim(), MessageFrom.Me);

                _PeerMessageTransmitter.QueueToTransmitPeerMessage(new TransmitToPeerClientMessage(_CurrentMessageTextBox.Text.Trim(), (_PeersList.SelectedItem as PeerDataViewModel)?.PeerName ?? string.Empty, _ClientRegistrationId, 1));
            }
            _CurrentMessageTextBox.Clear();
        }

        private void _CurrentMessageTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) && e.Key == Key.Enter)
            {
                _CurrentMessageTextBox.Text += "\n";
                _CurrentMessageTextBox.CaretIndex = _CurrentMessageTextBox.Text.Length;
            }
            else if(e.Key == Key.Enter)
            {
                sendMessage();
            }
        }

        public void HandleSignInResultServerMessage(SignInResultServerMessage signInResultServerMessage, IPeerMessageTransmitter clientAppState)
        {
            Dispatcher.Invoke(() =>
            {
                _PeerMessageTransmitter = clientAppState;
                if (signInResultServerMessage.Result)
                {
                    MessageBox.Show("Login successful", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    RequestRegistrationIdFromUser("Invalid credentials entered. Please try different credentials");
                }
            });
        }
    }
}
