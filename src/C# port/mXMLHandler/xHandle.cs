using System.Xml;
using System.Xml.Schema;
using System.IO;
using Doe.Exceptions;

namespace Doe.mXMLHandler
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

            xs.Add("",schemaPath);

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
                if (temp.InnerText.Equals(plugName))
                    return true;
            }

            return false;
        }

        public string[] getPlug(string s)
        {
            XmlElement root = plugDoc.DocumentElement;
            XmlNodeList plugList = root.SelectNodes("/Plugins/Plugin");

            string[] plugDet = new string[5];

            foreach (XmlNode temp in plugList)
            {
                XmlElement plugName = (XmlElement)temp.ChildNodes[0];
                if (s.Equals(plugName.InnerText))
                {
                    XmlElement plugLocation = (XmlElement)temp.ChildNodes[1];
                    XmlElement plugClass = (XmlElement)temp.ChildNodes[2];
                    XmlElement plugCompat = (XmlElement)temp.ChildNodes[4];
                    XmlElement stage = (XmlElement)temp.ChildNodes[6];
                    plugDet[0] = plugName.InnerText;
                    plugDet[1] = plugLocation.InnerText;
                    plugDet[2] = plugClass.InnerText;
                    plugDet[3] = plugCompat.InnerText;
                    plugDet[4] = stage.InnerText;
                }
            }

            return plugDet;
        }

        public static void customEHandle(object sender, ValidationEventArgs args)
        {
            isValid = false;
        }
    }
}
