using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktopUI.ViewModels
{
    public class PeersListViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PeerDataViewModel> _PeersList = new ObservableCollection<PeerDataViewModel>();
        private ConcurrentDictionary<string, PeerDataViewModel> _PeerDict = new ConcurrentDictionary<string, PeerDataViewModel>();

        public ObservableCollection<PeerDataViewModel> PeersList
        {
            get => _PeersList; set
            {
                _PeersList = value;
                onPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void MakeUserAvailable(string registrationId)
        {
            if (_PeerDict.ContainsKey(registrationId))
            {
                _PeerDict[registrationId].IsAvailable = true;
            }
            else
            {
                _PeerDict[registrationId] = new PeerDataViewModel(registrationId);
                PeersList.Add(_PeerDict[registrationId]);
            }
        }

        public void MakeUserNonAvailable(string registrationId)
        {
            if (_PeerDict.ContainsKey(registrationId))
            {
                _PeerDict[registrationId].IsAvailable = false;
            }
            else
            {
                //Not expected block
            }
        }

        public void AddMessage(string registrationId, string message, MessageFrom messageFrom)
        {
            if (_PeerDict.ContainsKey(registrationId))
            {
                _PeerDict[registrationId].ChatMessages.Add(new ChatMessageViewModel((messageFrom == MessageFrom.Me ? "Me : " : registrationId + " : ") + message));
            }
            else
            {
                //Not expected block
            }
        }
    }

    public enum MessageFrom
    {
        Me, Them
    }
}
