using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Astrarium.Plugins.Simbad
{
    public enum Primitives { VoBoolean, VoBit, VoUnsignedByte, VoShort, VoInt, VoLong, VoChar, VoUnicodeChar, VoFloat, VoDouble, VoFloatComplex, VoDoubleComplex, VoUndefined };

    public class VoTable
    {
        public List<VoColumn> Columns { get; private set; } = new List<VoColumn>();
        public List<VoRow> Rows { get; private set; } = new List<VoRow>();

        public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; private set; }

        private Dictionary<string, VoColumn> columns = new Dictionary<string, VoColumn>();

        public VoTable(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);
            LoadFromXML(doc);
        }
        
        private void LoadFromXML(XmlDocument xml)
        {
            string defaultNamespace = xml.DocumentElement.GetNamespaceOfPrefix("");
            var nsManager = new XmlNamespaceManager(xml.NameTable);
            nsManager.AddNamespace("vot", defaultNamespace);

            try
            {
                XmlNode table = xml.SelectSingleNode("/vot:VOTABLE/vot:RESOURCE/vot:TABLE", nsManager);
                if (table != null)
                {
                    foreach (XmlNode node in table.ChildNodes)
                    {
                        if (node.Name == "FIELD")
                        {
                            VoColumn col = new VoColumn(node);
                            columns.Add(col.Name, col);
                            Columns.Add(col);
                        }
                    }

                    XmlNode tableData = xml.SelectSingleNode("/vot:VOTABLE/vot:RESOURCE/vot:TABLE/vot:DATA/vot:TABLEDATA", nsManager);
                    if (tableData != null)
                    {
                        foreach (XmlNode node in tableData.ChildNodes)
                        {
                            if (node.Name == "TR")
                            {
                                var row = new VoRow(node);
                                Rows.Add(row);
                            }
                        }
                    }

                    if (!Rows.Any())
                    {
                        ErrorMessage = "There are no data";
                    }
                }
                else
                {
                    var info = xml.SelectSingleNode("/vot:VOTABLE/vot:INFO", nsManager);
                    if (info != null)
                    {
                        string name = info.Attributes?["name"]?.Value;
                        string value = info.Attributes?["value"]?.Value;
                        if (name == "Error")
                        {
                            ErrorMessage = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }

    public class VoRow
    {
        private object[] columnData;

        public VoRow(XmlNode node) 
        {
            columnData = new object[node.ChildNodes.Count];
            int index = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                columnData[index++] = child.InnerText.Trim();
            }
        }

        public object GetCellValue(int index) { return columnData[index]; }
    }

    public class VoColumn
    {
        public string Id { get; private set; }
        public Primitives Type { get; private set; }
        public int Precision { get; private set; }
        public int Dimensions { get; private set; }
        public string Description { get; set; }
        public int[] Sizes = null;
        public string Ucd { get; private set; }
        public string Unit { get; private set; }
        public string Name { get; private set; }
  
        public VoColumn(XmlNode node)
        {
            if (node.Attributes["datatype"] != null)
            {
                Type = GetType(node.Attributes["datatype"].Value);
            }
            if (node.Attributes["ucd"] != null)
            {
                Ucd = node.Attributes["ucd"].Value;
            }
            if (node.Attributes["precision"] != null)
            {
                try
                {
                    Precision = Convert.ToInt32(node.Attributes["precision"].Value);
                }
                catch { }
            }
            if (node.Attributes["ID"] != null)
            {
                Id = node.Attributes["ID"].Value;
            }

            if (node.Attributes["name"] != null)
            {
                Name = node.Attributes["name"].Value;
            }
            else
            {
                Name = Id;
            }

            if (node.Attributes["unit"] != null)
            {
                Unit = node.Attributes["unit"].Value;
            }

            if (node.Attributes["arraysize"] != null)
            {
                string[] split = node.Attributes["arraysize"].Value.Split(new char[] { 'x' });
                Dimensions = split.GetLength(0);
                Sizes = new int[split.GetLength(0)];
                int indexer = 0;
                foreach (string dim in split)
                {
                    if (!dim.Contains("*"))
                    {
                        Sizes[indexer++] = Convert.ToInt32(dim);
                    }
                    else
                    {
                        int len = 9999;
                        string lenString = dim.Replace("*", "");
                        if (lenString.Length > 0)
                        {
                            len = Convert.ToInt32(lenString);
                        }
                        Sizes[indexer++] = len;

                    }
                }
            }

            Description = node.InnerText;
        }

        public static Primitives GetType(string type)
        {
            Primitives Type = Primitives.VoUndefined;
            switch (type)
            {
                case "boolean":
                    Type = Primitives.VoBoolean;
                    break;
                case "bit":
                    Type = Primitives.VoBit;
                    break;
                case "unsignedByte":
                    Type = Primitives.VoUnsignedByte;
                    break;
                case "short":
                    Type = Primitives.VoShort;
                    break;
                case "int":
                    Type = Primitives.VoInt;
                    break;
                case "long":
                    Type = Primitives.VoLong;
                    break;
                case "char":
                    Type = Primitives.VoChar;
                    break;
                case "unicodeChar":
                    Type = Primitives.VoUnicodeChar;
                    break;
                case "float":
                    Type = Primitives.VoFloat;
                    break;
                case "double":
                    Type = Primitives.VoDouble;
                    break;
                case "floatComplex":
                    Type = Primitives.VoFloatComplex;
                    break;
                case "doubleComplex":
                    Type = Primitives.VoDoubleComplex;
                    break;
                default:
                    Type = Primitives.VoUndefined;
                    break;
            }
            return Type;
        }

        public override string ToString()
        {
            return Name;
        }

        public object ParseValue(object value)
        {
            switch (Type)
            {
                case Primitives.VoDouble:
                    return double.Parse(value.ToString(), CultureInfo.InvariantCulture);
                case Primitives.VoFloat:
                    return float.Parse(value.ToString(), CultureInfo.InvariantCulture);
                case Primitives.VoShort:
                    return short.Parse(value.ToString(), CultureInfo.InvariantCulture);
                case Primitives.VoInt:
                    return int.Parse(value.ToString(), CultureInfo.InvariantCulture);
                case Primitives.VoChar:
                    return value.ToString();
                default:
                    return value;
            }
        }
    }
}
