using SocketFrm;
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
            ClientsManager clientsManager = new ClientsManager(serverUINotifier);
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 2060);
            tcpListener.Start();

            Action listenForNewConnections = new Action(() =>
            {
                while (true)
                {
                    Console.WriteLine("Listening to socket...");
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

        private static void handleTcpClient(TcpClient tcpClient)
        {
            Console.WriteLine("\tSocket accepted. Processing further");
            NetworkStream networkStream = tcpClient.GetStream();
            //readSimpleString(networkStream);
            readSocketCommandData(networkStream);

            networkStream.Close();

            tcpClient.Close();
        }

        private static void readSocketCommandData(NetworkStream networkStream)
        {
            ClientMessage socketCommand = ClientMessage.Deserialize(networkStream);

            handleSocketCommand(socketCommand);
        }

        private static void handleSocketCommand(ClientMessage socketCommand)
        {
            switch (socketCommand.ClientMessageType)
            {
                case ClientMessageType.DisplayTextToConsole:
                    displayTextToConsole(socketCommand as DisplayTextClientMessage);
                    break;
                default:
                    break;
            }
        }

        private static void displayTextToConsole(DisplayTextClientMessage displayTextSocketCommand)
        {
            Console.WriteLine(displayTextSocketCommand.DisplayText);
        }

        private static void readSimpleString(NetworkStream networkStream)
        {
            StreamReader reader = new StreamReader(networkStream);
            StreamWriter writer = new StreamWriter(networkStream);
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                Console.WriteLine("\t" + line);
            }
        }
    }
}
