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
                TcpListener tcpListener = new TcpListener(iPAddress,2060);

                tcpListener.Start();

                while (true)
                {
                    Console.WriteLine("Listening to socket...");
                    Socket socket = tcpListener.AcceptSocket();
                    Task socketHandlerTask = new Task(someSocket => handleSocket(someSocket as Socket), socket);
                    socketHandlerTask.ContinueWith((taskObj) => Console.WriteLine("\tFinished executing the socket handler."));
                    socketHandlerTask.Start();
                }
            }

            

            Console.ReadKey();
        }

        private static void handleSocket(Socket socket)
        {
            Console.WriteLine("\tSocket accepted. Processing further");
            NetworkStream networkStream = new NetworkStream(socket);

            StreamReader reader = new StreamReader(networkStream);
            StreamWriter writer = new StreamWriter(networkStream);
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                Console.WriteLine("\t" + line);
            }

            networkStream.Close();

            socket.Close();
        }
    }
}
