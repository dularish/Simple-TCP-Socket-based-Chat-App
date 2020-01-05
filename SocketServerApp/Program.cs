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
    class Program
    {
        static void Main(string[] args)
        {
            string domainName = Dns.GetHostName();

            IPAddress iPAddress = null;

            foreach (var addressItem in Dns.GetHostEntry(domainName).AddressList)
            {
                Console.WriteLine("{0}/{1}", domainName, addressItem);
                if (addressItem.AddressFamily == AddressFamily.InterNetwork)
                {
                    iPAddress = addressItem;
                    break;
                }
            }

            if(iPAddress != null)
            {
                ClientsManager clientsManager = new ClientsManager(new ConsoleNotifier());
                TcpListener tcpListener = new TcpListener(iPAddress,2060);

                tcpListener.Start();

                while (true)
                {
                    Console.WriteLine("Listening to socket...");
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    clientsManager.AcceptClient(tcpClient);
                    //Task socketHandlerTask = new Task(someTcpClient => handleTcpClient(someTcpClient as TcpClient), tcpClient);
                    //socketHandlerTask.ContinueWith((taskObj) => Console.WriteLine("\tFinished executing the socket handler."));
                    //socketHandlerTask.Start();
                }
            }

            

            Console.ReadKey();
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
