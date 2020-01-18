using ClientDesktopUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class MainWindow : Window
    {
        public PeersListViewModel PeersListViewModel = new PeersListViewModel();
        public MainWindow()
        {
            InitializeComponent();
            PeersListViewModel.PeersList.Add(new PeerDataViewModel("FirstPeer"));
            PeersListViewModel.PeersList.Add(new PeerDataViewModel("SecondPeer"));
            DataContext = PeersListViewModel;
            Task.Run(() => { System.Threading.Thread.Sleep(10000); Dispatcher.Invoke(() => PeersListViewModel.PeersList.Add(new PeerDataViewModel("ThirdPeer"))); });
        }

        private void _SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_CurrentMessageTextBox.Text))
            {
                (_PeersList.SelectedItem as PeerDataViewModel)?.ChatMessages.Add(_CurrentMessageTextBox.Text.Trim());
            }
            _CurrentMessageTextBox.Clear();
        }
    }
}
