using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SocketFrm
{
    public enum ServerMessageType { DisplayTextToConsole, RegisterIdResult, ClientAvailabilityNotification, TransmitToPeerResult }

    [DataContract]
    [KnownType(typeof(DisplayTextServerMessage))]
    [KnownType(typeof(RegisterIdResultServerMessage))]
    [KnownType(typeof(ClientAvailabilityNotificationServerMessage))]
    [KnownType(typeof(TransmitToPeerResultServerMessage))]
    public abstract class ServerMessage
    {
        [DataMember]
        private ServerMessageType _ServerMessageType;

        public ServerMessageType ServerMessageType { get => _ServerMessageType; protected set => _ServerMessageType = value; }

        public byte[] Serialize(out int length)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(s => typeof(ServerMessage).IsAssignableFrom(s));

            MemoryStream memoryStream = new MemoryStream();
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ServerMessage), allTypes);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.Encoding = new UnicodeEncoding();
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
            dataContractSerializer.WriteObject(xmlWriter, this);
            xmlWriter.Flush();
            byte[] buffer = memoryStream.GetBuffer();
            length = (int)memoryStream.Length;
            xmlWriter.Close();
            memoryStream.Close();
            return buffer;
        }

        public static ServerMessage Deserialize(StreamReader streamReader)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(s => typeof(ServerMessage).IsAssignableFrom(s));

            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ServerMessage), allTypes);
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment
                //Conformance level is fragment because we have multiple xmls in the same stream
            };
            
            XmlReader xmlReader = XmlReader.Create(streamReader, xmlReaderSettings);
            XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
            var data = dataContractSerializer.ReadObject(xmlReader);
            xmlDictionaryReader.Close();
            xmlReader.Close();
            //streamReader.Close();//Do not close the streamReader here

            return data as ServerMessage;
        }
    }
}
