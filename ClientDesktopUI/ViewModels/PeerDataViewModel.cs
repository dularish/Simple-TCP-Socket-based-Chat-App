using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktopUI.ViewModels
{
    public class PeerDataViewModel : INotifyPropertyChanged
    {
        private string _PeerName;

        private ObservableCollection<string> _ChatMessages = new ObservableCollection<string>();

        public string PeerName
        {
            get => _PeerName; set
            {
                _PeerName = value;
                onPropertyChanged();
            }
        }

        public ObservableCollection<string> ChatMessages
        {
            get => _ChatMessages; set
            {
                _ChatMessages = value;
                onPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PeerDataViewModel(string peerName)
        {
            PeerName = peerName;
            ChatMessages.Add(peerName + " says hello");
            ChatMessages.Add(peerName + " says goodbye");
        }
    }
}
