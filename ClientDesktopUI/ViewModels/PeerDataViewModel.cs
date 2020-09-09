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
        private bool _IsAvailable;

        private ObservableCollection<ChatMessageViewModel> _ChatMessages = new ObservableCollection<ChatMessageViewModel>();

        public string PeerName
        {
            get => _PeerName; set
            {
                _PeerName = value;
                onPropertyChanged();
            }
        }

        public ObservableCollection<ChatMessageViewModel> ChatMessages
        {
            get => _ChatMessages; set
            {
                _ChatMessages = value;
                onPropertyChanged();
            }
        }

        public bool IsAvailable
        {
            get => _IsAvailable; set
            {
                _IsAvailable = value;
                onPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PeerDataViewModel(string peerName, bool isAvailable = true)
        {
            PeerName = peerName;
            IsAvailable = isAvailable;
        }
    }
}
