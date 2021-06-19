using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Start_a_Town_
{
    static public class XmlHelper
    {
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            using (var reader = xDocument.CreateReader())
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);
                return xml;
            }
        }
        public static string GetValueOrDefault(this XElement root, string name, string defaultVal)
        {
            var node = root.Descendants(name).FirstOrDefault();
            return node != null ? node.Value : defaultVal;
        }
        public static bool TryGetValue(this XElement root, string name, Action<string> valueFunc)
        {
            var node = root.Descendants(name).FirstOrDefault();
            if (node != null)
            {
                valueFunc(node.Value);
                return true;
            }
            return false;
        }
    }
}
