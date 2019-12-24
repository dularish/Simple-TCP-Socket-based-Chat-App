using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SocketFrm;

namespace SimpleClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient(Dns.GetHostName(), 2060);

            try
            {
                //writeSimpleStringToStream(tcpClient);
                writeSimpleCommandToStream(tcpClient);
            }
            finally
            {
                tcpClient.Close();
            }

        }

        private static void writeSimpleCommandToStream(TcpClient tcpClient)
        {
            DisplayTextSocketCommand displayTextSocketCommand = new DisplayTextSocketCommand("Some simple string");

            NetworkStream networkStream = tcpClient.GetStream();

            byte[] dataBuffer = displayTextSocketCommand.Serialize(out int dataLength);

            //byte[] writeBuffer = new byte[sizeof(int)];
            //writeBuffer = BitConverter.GetBytes((int)dataLength);
            //networkStream.Write(writeBuffer, 0, sizeof(int));
            //networkStream.Flush();

            networkStream.Write(dataBuffer, 0, dataLength);
            networkStream.Flush();

            networkStream.Close();
        }

        private static void writeSimpleStringToStream(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            StreamWriter streamWriter = new StreamWriter(networkStream) { AutoFlush = true };
            streamWriter.Write("Hello from client");
            networkStream.Close();
        }

        private static string getLocalIPAddress()
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

            return iPAddress.ToString();
        }
    }
}
