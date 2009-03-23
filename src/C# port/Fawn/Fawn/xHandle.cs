using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace Fawn
{
    public class xHandle
    {
        XmlDocument plugDoc = new XmlDocument();
        string xmlPath;
        string schemaPath;
        private static bool isValid = true;

        public xHandle(string docPath, string scPath) //sets up doc and schema path and loads xml doc
        {
            xmlPath = docPath;
            schemaPath = scPath;
            plugDoc.Load(docPath);
        }

        public bool verify()
        {
            if (!(File.Exists(xmlPath) && File.Exists(schemaPath)))
            {
                throw new customException("Schema or Config corrupted or doesn't exist");
            }
            //code to check if config matches schema
            XmlSchemaSet xs = new XmlSchemaSet();
            XmlSchema X = new XmlSchema();

            xs.Add("", schemaPath);

            XmlReaderSettings setting = new XmlReaderSettings();
            setting.ValidationType = ValidationType.Schema;
            setting.Schemas = xs;
            setting.ValidationEventHandler += new ValidationEventHandler(customEHandle);

            XmlReader xr = XmlReader.Create(xmlPath, setting);

            while (xr.Read()) ;
            return isValid;
        }

        public bool checkPlug(string plugName)
        {
            XmlElement root = plugDoc.DocumentElement;
            XmlNodeList plugList = root.SelectNodes("/Plugins/Plugin/PlugName");

            foreach (XmlNode temp in plugList)
            {
                if (temp.Value.Equals(plugName))
                    return true;
            }

            return false;
        }

        public int getCount(string s)
        {
            XmlElement root = plugDoc.DocumentElement;
            XmlNodeList divlist = root.SelectNodes(s);
            return divlist.Count;
        }

        public XmlNodeList getNodes(string s)
        {
            XmlElement root = plugDoc.DocumentElement;
            XmlNodeList plugList = root.SelectNodes(s);
            return plugList;
        }

        public static void customEHandle(object sender, ValidationEventArgs args)
        {
            isValid = false;
        }
    }
}
