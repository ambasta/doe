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

            //write code to check if config matches schema
            return true;
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

        public string[] getPlug(string s)
        {
            XmlElement root = plugDoc.DocumentElement;
            XmlNodeList plugList = root.SelectNodes("/Plugins/Plugin");

            string[] plugDet = new string[5];

            foreach (XmlNode temp in plugList)
            {
                XmlElement plugName = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugName");
                if (s.Equals(plugName))
                {
                    XmlElement plugLocation = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugLocation");
                    XmlElement plugClass = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugClass");
                    XmlElement plugCompat = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugCompat");
                    XmlElement stage = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/Stage");
                    plugDet[0] = plugName.Value;
                    plugDet[1] = plugLocation.Value;
                    plugDet[2] = plugClass.Value;
                    plugDet[3] = plugCompat.Value;
                    plugDet[4] = stage.Value;
                }
            }

            return plugDet;
        }
    }
}
