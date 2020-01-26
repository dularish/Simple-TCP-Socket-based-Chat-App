using SocketFrm;
using SocketServerApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServerWinService
{
    partial class ChatServerService : ServiceBase, IServerUINotifier
    {
        private ManualResetEvent _serverWantsShutdown = new ManualResetEvent(false);

        public ChatServerService()
        {
            InitializeComponent();
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists(nameof(ChatServerService)))
            {
                EventLog.CreateEventSource(nameof(ChatServerService), string.Empty);
            }
            eventLog1.Source = nameof(ChatServerService);
            eventLog1.Log = string.Empty;
        }

        public ManualResetEvent ServerWantsShutdown => _serverWantsShutdown;

        public void HandleDisplayTextToConsoleMessage(DisplayTextClientMessage displayTextClientMessage)
        {
            eventLog1.WriteEntry(displayTextClientMessage.DisplayText);
        }

        public void HandleRegisterIDInServerMessage(RegisterIdClientMessage registerIdClientMessage)
        {
            eventLog1.WriteEntry("Handling registering of client with ID:" + registerIdClientMessage.ID);
        }

        public void HandleTransmitToPeerMessage(TransmitToPeerClientMessage transmitToPeerClientMessage)
        {
            //Not logging
        }

        public void NotifyClientDisconnection(string clientDisconnectedID)
        {
            eventLog1.WriteEntry("Disconnecting client :" + clientDisconnectedID);
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Begin OnStart");
            Task tcpListenerStartTask = ChatServerProgram.StartTCPListener(this);
            eventLog1.WriteEntry("End Onstart");
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Begin OnStop");
            _serverWantsShutdown.Set();
            eventLog1.WriteEntry("End OnStop");
        }
    }
}
