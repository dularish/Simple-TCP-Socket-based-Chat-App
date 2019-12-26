using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SocketFrm;
using System.Collections.Concurrent;
using System.Timers;

namespace SimpleClientApp
{
    class Program
    {
        private static ConcurrentQueue<ClientMessage> _clientMessagesQueue = new ConcurrentQueue<ClientMessage>();

        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient(Dns.GetHostName(), 2060);

            try
            {
                //writeSimpleStringToStream(tcpClient);
                //writeSimpleCommandToStream(tcpClient);
                Task readStreamTask = new Task((someTcpClientObj) => ReadStream(someTcpClientObj as TcpClient), tcpClient);
                readStreamTask.Start();
                Task writeStreamTask = new Task((someTcpClientObj) => WriteStream(someTcpClientObj as TcpClient), tcpClient);
                writeStreamTask.Start();
                Task.Run(() => RandomlyQueueClientMessages());

                Task.WhenAny(new List<Task>() { readStreamTask, writeStreamTask }).Wait();
            }
            finally
            {
                tcpClient.Client.Shutdown(SocketShutdown.Both);
                tcpClient.Close();
            }

        }

        private static void RandomlyQueueClientMessages()
        {
            Random randGen = new Random(36);
            Timer timer = new Timer(randGen.Next(3000,6000));
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ClientMessage clientMessage = new DisplayTextClientMessage("Client says Now the time is " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
            _clientMessagesQueue.Enqueue(clientMessage);
        }

        private static void WriteStream(TcpClient tcpClient)
        {
            while (true)
            {
                if (!_clientMessagesQueue.IsEmpty)
                {
                    if(_clientMessagesQueue.TryDequeue(out ClientMessage clientMessage))
                    {
                        try
                        {
                            NetworkStream networkStream = tcpClient.GetStream();

                            byte[] bufferData = clientMessage.Serialize(out int dataLength);

                            networkStream.Write(bufferData, 0, dataLength);
                            networkStream.Flush();

                        }
                        catch (IOException ioEx)
                        {
                            Console.WriteLine("Server was forcibly closed during the write process.. :(");
                            Console.WriteLine("Message : " + ioEx.Message);
                            break;
                        }
                        catch(InvalidOperationException iopEx)
                        {
                            Console.WriteLine("Read process : Server had forcibly close the connection before .. :(");
                            Console.WriteLine("Message : " + iopEx.Message);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.GetType().ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
                            
                            break;
                        }
                    }
                }
            }
            Console.ReadKey();
        }

        private static void ReadStream(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();

            while (true)
            {
                try
                {
                    if (networkStream.DataAvailable)
                    {
                        ServerMessage serverMessage = ServerMessage.Deserialize(new StreamReader(networkStream));

                        handleServerMessage(serverMessage);
                    }
                }
                catch(IOException ioEx)
                {
                    Console.WriteLine("Server was forcibly closed during the read process.. :(");
                    Console.WriteLine("Message : " + ioEx.Message);
                    break;
                }
                catch (InvalidOperationException iopEx)
                {
                    Console.WriteLine("Write process : Server had forcibly close the connection before .. :(");
                    Console.WriteLine("Message : " + iopEx.Message);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType().ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
                    break;
                }
            }
            Console.ReadKey();
        }

        private static void handleServerMessage(ServerMessage serverMessage)
        {
            switch (serverMessage.ServerMessageType)
            {
                case ServerMessageType.DisplayTextToConsole:
                    displayTextToConsole(serverMessage as DisplayTextServerMessage);
                    break;
                default:
                    break;
            }
        }

        private static void displayTextToConsole(DisplayTextServerMessage displayTextServerMessage)
        {
            Console.WriteLine(displayTextServerMessage.DisplayText);
        }

        private static void writeSimpleCommandToStream(TcpClient tcpClient)
        {
            DisplayTextClientMessage displayTextSocketCommand = new DisplayTextClientMessage("Some simple string");

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
