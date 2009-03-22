using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;



namespace Doe.XMLReadWrite
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
            if (!File.Exists(path))
                return "config404";                              //error 404
            if (File.Exists(fileName))
                return "schema404";                              //error schemaNotFound
            return "Valid";
        }

        public String[,] loadPlug()
        {
            //load xml document
            plugDoc = new XmlDocument();
            plugDoc.Load(path);

            //select root and then nodelist from the root
            XmlElement root = plugDoc.DocumentElement;
            XmlNodeList plugin = root.SelectNodes("/Plugins/Plugin");

            //create as many plugins as plugin nodes in root
            String[,] plugins = new String[plugin.Count, 4];
            int count = 0;
            foreach (XmlNode temp in plugin)
            {
                XmlElement plugName = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugName");
                XmlElement plugLocation = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugLocation");
                XmlElement plugCompat = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/PlugCompat");
                XmlElement stage = (XmlElement)temp.SelectSingleNode("/Plugins/Plugin/Stage");
                plugins[count, 0] = plugName.Value;
                plugins[count, 1] = plugLocation.Value;
                plugins[count, 2] = plugCompat.Value;
                plugins[count, 3] = stage.Value;
                count++;
            }

            //return plugins as name, location, compat and stage
            return plugins;
        }
    }
}