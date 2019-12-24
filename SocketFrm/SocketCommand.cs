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
    public enum CommandType { DisplayTextToConsole}

    [DataContract]
    [KnownType(typeof(DisplayTextSocketCommand))]
    public abstract class SocketCommand
    {
        [DataMember]
        private CommandType _CommandType;

        public CommandType CommandType { get => _CommandType; }

        public byte[] Serialize(out int length)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(s => typeof(SocketCommand).IsAssignableFrom(s));

            MemoryStream memoryStream = new MemoryStream();
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(SocketCommand), allTypes);
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

        public static SocketCommand Deserialize(StreamReader streamReader)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(s => typeof(SocketCommand).IsAssignableFrom(s));

            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(SocketCommand), allTypes);
            XmlReader xmlReader = XmlReader.Create(streamReader);
            XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
            var data = dataContractSerializer.ReadObject(xmlDictionaryReader);
            xmlDictionaryReader.Close();
            xmlReader.Close();
            streamReader.Close();

            return data as SocketCommand;
        }

    }
}
