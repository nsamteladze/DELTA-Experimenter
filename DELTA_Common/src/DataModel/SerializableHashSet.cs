using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DELTA_Common.DataModel
{
    [Serializable, XmlRoot("hashset")]
    public class SerializableHashSet<T>
        : HashSet<T>, IXmlSerializable
    {
        #region Contructors

        public SerializableHashSet() : base() { }

        public SerializableHashSet(IEnumerable<T> collection) : base(collection) { }

        #endregion

        #region IXmlSerializable Implementation

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty) return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                T item = (T)serializer.Deserialize(reader);
                this.Add(item);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            foreach (T item in this)
            {
                writer.WriteStartElement("item");
                serializer.Serialize(writer, item);
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
