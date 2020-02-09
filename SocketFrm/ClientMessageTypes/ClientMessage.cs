using SocketFrm.ClientMessageTypes;
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
    public enum ClientMessageType { DisplayTextToConsole, RegisterIDInServer, TransmitToPeer, SignIn }

    [Serializable]
    [DataContract]
    [KnownType(typeof(DisplayTextClientMessage))]
    [KnownType(typeof(RegisterIdClientMessage))]
    [KnownType(typeof(TransmitToPeerClientMessage))]
    [KnownType(typeof(SignInClientMessage))]
    public abstract class ClientMessage
    {
        [DataMember]
        private ClientMessageType _ClientMessageType;

        public ClientMessageType ClientMessageType { get => _ClientMessageType; protected set => _ClientMessageType = value; }

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

        public static ClientMessage Deserialize(NetworkStream networkStream)
        {
            ClientMessage message = null;

            byte[] dataSizeBytes = new byte[4];
            int readBytes = networkStream.Read(dataSizeBytes, 0, 4);
            int dataSize = BitConverter.ToInt32(dataSizeBytes, 0);


            BinaryReader binaryReader = new BinaryReader(networkStream);
            byte[] data = binaryReader.ReadBytes(dataSize);
            message = Deserialize(data);

            return message;
        }

        public static ClientMessage Deserialize(byte[] data)
        {
            ClientMessage message;
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                message = (ClientMessage)bf.Deserialize(ms);
            }

            return message;
        }
    }
}
