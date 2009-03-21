using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace doe.XMLReadWrite
{
    public class xmlManager   //validates xml + reads + writes
    {
        XmlDocument plugDoc;
        String path;

        public xmlManager(String paths)
        {
            path = paths;
        }

        public String verifyPlug(String fileName)
        {
            plugDoc = new XmlDocument();
            XmlTextReader xReader;
            if (File.Exists(path))
            {
                plugDoc.Load(path);
                xReader = new XmlTextReader(path);
            }
            else
                return "404";                              //error 404
            if (File.Exists(fileName))
            {
                xReader = new XmlTextReader(path);
                XmlSchema plugSchema = new XmlSchema.Read(xReader, new ValidationEventHandler(SchemaReadError));
            }
            return "valid";
        }

        public String[,] loadPlug()
        {
            XmlElement root = plugDoc.DocumentElement;
            XmlNodeList plugin = root.SelectNodes("/Plugins/Plugin");
            String[,] plugins = new String[plugin.Count,4];
            int count = 0;
            foreach(XmlNode temp in plugin)
            {
                XmlElement plugName = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugName");
                XmlElement plugLocation = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugLocation");
                XmlElement plugCompat = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugCompat");
                XmlElement stage = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/Stage");
                plugins[count,0] = plugName.Value;
                plugins[count,1] = plugLocation.Value;
                plugins[count,2] = plugCompat.Value;
                plugins[count,3] = stage.Value;
                count++;
            }
            return plugins;
        }
    }
}
