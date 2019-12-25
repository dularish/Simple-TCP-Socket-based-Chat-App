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
    public enum ClientMessageType { DisplayTextToConsole}

    [DataContract]
    [KnownType(typeof(DisplayTextClientMessage))]
    public abstract class ClientMessage
    {
        [DataMember]
        private ClientMessageType _ClientMessageType;

        public ClientMessageType ClientMessageType { get => _ClientMessageType; protected set => _ClientMessageType = value; }

        public byte[] Serialize(out int length)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(s => typeof(ClientMessage).IsAssignableFrom(s));

            MemoryStream memoryStream = new MemoryStream();
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ClientMessage), allTypes);
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

        public static ClientMessage Deserialize(StreamReader streamReader)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(s => typeof(ClientMessage).IsAssignableFrom(s));

            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ClientMessage), allTypes);
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment
                //Conformance level is fragment because we have multiple xmls in the same stream
            };
            XmlReader xmlReader = XmlReader.Create(streamReader, xmlReaderSettings);
            XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
            var data = dataContractSerializer.ReadObject(xmlDictionaryReader);
            xmlDictionaryReader.Close();
            xmlReader.Close();
            //streamReader.Close();//Do not close the streamReader here

            return data as ClientMessage;
        }

    }
}
