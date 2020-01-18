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
    public class PeersListViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PeerDataViewModel> _PeersList = new ObservableCollection<PeerDataViewModel>();

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
    }
}
