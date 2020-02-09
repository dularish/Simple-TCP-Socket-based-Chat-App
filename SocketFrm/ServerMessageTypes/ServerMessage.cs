using SocketFrm.ServerMessageTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SocketFrm
{
    public enum ServerMessageType { DisplayTextToConsole, RegisterIdResult, ClientAvailabilityNotification, TransmitToPeerResult, TransmitToPeer, KeepAlive, SignInResult }

    [Serializable]
    [DataContract]
    [KnownType(typeof(DisplayTextServerMessage))]
    [KnownType(typeof(RegisterIdResultServerMessage))]
    [KnownType(typeof(ClientAvailabilityNotificationServerMessage))]
    [KnownType(typeof(TransmitToPeerResultServerMessage))]
    [KnownType(typeof(SignInResultServerMessage))]
    public abstract class ServerMessage
    {
        [DataMember]
        private ServerMessageType _ServerMessageType;

        public ServerMessageType ServerMessageType { get => _ServerMessageType; protected set => _ServerMessageType = value; }

        public byte[] Serialize(out int length)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memoryStream, this);
            byte[] data = memoryStream.ToArray();

            byte[] sizeOfData = BitConverter.GetBytes(data.Length);
            length = data.Length + 4;
            return sizeOfData.Concat(data).ToArray();
        }

        public static ServerMessage Deserialize(NetworkStream networkStream)
        {
            ServerMessage message = null;

            byte[] dataSizeBytes = new byte[4];
            int readBytes = networkStream.Read(dataSizeBytes, 0, 4);
            int dataSize = BitConverter.ToInt32(dataSizeBytes, 0);


            BinaryReader binaryReader = new BinaryReader(networkStream);
            byte[] data = binaryReader.ReadBytes(dataSize);

            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                message = (ServerMessage)bf.Deserialize(ms);
            }

            return message;
        }
    }
}
