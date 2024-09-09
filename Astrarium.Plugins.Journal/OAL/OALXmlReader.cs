
using System.IO;
using System.Xml;

namespace Astrarium.Plugins.Journal.OAL
{
    internal class OALXmlReader : XmlNodeReader
    {
        public OALXmlReader(string filePath) : base(ParseDocument(filePath)) { }

        private static XmlDocument ParseDocument(string filePath)
        {
            var document = new XmlDocument();

            using (var xmlReader = new FilteredXmlReader(File.OpenRead(filePath)))
            {
                document.Load(xmlReader);
            }

            var namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("xsi", OALData.XSI);
            XmlNodeList nodes = document.SelectNodes("//*[@xsi:type]", namespaceManager);

            foreach (XmlNode node in nodes)
            {
                XmlAttribute typeAttr = node.Attributes["xsi:type"];
                // Take only types within "oal:" namespace
                if (!typeAttr.Value.StartsWith("oal:"))
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
            return document;
        }
    }

    internal class FilteredXmlReader : TextReader
    {
        private readonly StreamReader streamReader;

        public Stream BaseStream => streamReader.BaseStream;

        public FilteredXmlReader(Stream stream)
        {
            streamReader = new StreamReader(stream);
        }

        public override void Close()
        {
            streamReader.Close();
        }

        protected override void Dispose(bool disposing)
        {
            streamReader.Dispose();
        }

        public override int Peek()
        {
            var peek = streamReader.Peek();
            while (IsInvalid(peek, true))
            {
                streamReader.Read();
                peek = streamReader.Peek();
            }
            return peek;
        }

        public override int Read()
        {
            var read = streamReader.Read();
            while (IsInvalid(read, true))
            {
                read = streamReader.Read();
            }
            return read;
        }

        private bool IsInvalid(int c, bool invalidateCompatibilityCharacters)
        {
            if (c == -1)
            {
                return false;
            }
            if (invalidateCompatibilityCharacters && ((c >= 0x7F && c <= 0x84) || (c >= 0x86 && c <= 0x9F) || (c >= 0xFDD0 && c <= 0xFDEF)))
            {
                return true;
            }
            if (c == 0x9 || c == 0xA || c == 0xD || (c >= 0x20 && c <= 0xD7FF) || (c >= 0xE000 && c <= 0xFFFD))
            {
                return false;
            }
            return true;
        }
    }

}
