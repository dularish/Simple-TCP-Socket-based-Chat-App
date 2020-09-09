using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktopUI.ViewModels
{
    public class ChatMessageViewModel
    {
        public string MessageText { get; private set; }

        public ChatMessageViewModel(string messageText)
        {
            MessageText = messageText;
        }
    }
}
