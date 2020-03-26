using SocketFrm;
using SocketServerApp.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServerApp
{
    public class ChatServerProgram
    {
        static void Main(string[] args)
        {
            IServerUINotifier serverUINotifier = new ConsoleNotifier();
            Task tcpListenerStartTask = StartTCPListener(serverUINotifier);
            tcpListenerStartTask.Wait();
        }

        /// <summary>
        /// Returns a task that can be awaited for completion of ShutDown of Server
        /// </summary>
        /// <param name="serverUINotifier"></param>
        /// <returns></returns>
        public static Task StartTCPListener(IServerUINotifier serverUINotifier)
        {
            IAuthenticationService authenticationService = new SimplestAuthService();
            ClientsManager clientsManager = new ClientsManager(serverUINotifier, authenticationService);
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 2060);
            tcpListener.Start();

            Action listenForNewConnections = new Action(() =>
            {
                while (true)
                {
                    serverUINotifier.LogText("Listening to socket...");
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    clientsManager.AcceptClient(tcpClient);
                }
            });

            Task listenForNewConnectionsTask = new Task(listenForNewConnections);
            listenForNewConnectionsTask.Start();
            Task shutDownWaitTask = new Task(() => serverUINotifier.ServerWantsShutdown.WaitOne());
            shutDownWaitTask.Start();

            return Task.WhenAny(new List<Task>() { listenForNewConnectionsTask, shutDownWaitTask });
            
        }
    }
}
