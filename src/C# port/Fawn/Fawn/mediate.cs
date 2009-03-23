using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.Serialization.Formatters.Binary;

namespace Fawn
{
    class mediate
    {
        static void getFile(NetworkStream ns)
        {
            UInt64 fLen;
            int readBytes = 0;
            byte[] message = new byte[8];
            FileStream fs = new FileStream("Config.xml", FileMode.Create, FileAccess.Write);
            readBytes = ns.Read(message, 0, 8);
            fLen = BitConverter.ToUInt64(message,0);
            message = new byte[4096];
            do
            {
                readBytes = ns.Read(message, 0, 4096);
                fs.Write(message, 0, readBytes);
                fLen -= Convert.ToUInt64(readBytes);
            } while (fLen > 0);
            fs.Close();
            ns.Close();
        }

        static void sendData(NetworkStream ns)
        {
            xHandle input = new xHandle("input.xml", "input.xsd");
            try
            {
                if (!input.verify())
                {
                    Console.WriteLine("Bad Input File");
                    Environment.Exit(1);
                }
                Console.WriteLine("Loaded Input");
            }
            catch (customException e)
            {
                Console.WriteLine(e.ErrorMessage);
                Environment.Exit(1);
            }

            int divNum = input.getCount("/Inputs/Div");
            int plugNum = input.getCount("/Inputs/Plugin");

            string[] outData = new string[divNum+plugNum+1];
            outData[0] = Convert.ToString(divNum);

            int i = 1;
            XmlNodeList nodes = input.getNodes("/Inputs/Div");

            foreach (XmlNode node in nodes)
            {
                XmlElement divId = (XmlElement)node;
                outData[i] = "Div$" + divId.Value;
                i++;
            }
                //each element contains div$divid

            nodes = input.getNodes("/Inputs/Plugin");
            foreach (XmlNode node in nodes)
            {
                //each element contains plugin$pluginName$Args*Args*Args..
                XmlElement plugId = (XmlElement)node.SelectSingleNode("/Inputs/Plugin/PlugName");
                outData[i] = "Plugin$" + plugId.Value + "$";
                XmlNodeList plugArg = node.SelectNodes("/Input/Plugin/plugInput");
                foreach (XmlNode temp in plugArg)
                {
                    plugId = (XmlElement)temp;
                    outData[i] += plugId.Value+"*";
                }
                outData[i] = outData[i].Substring(0, outData[i].Length - 1);
                i++;
            }

            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, (object)outData);
            byte[] reply = ms.ToArray();

            ns.Write(reply, 0, reply.Length);
        }

        static void recvData(NetworkStream ns)
        {
            while (true)
            {
                byte[] buffer = new byte[4096];
                int rb = ns.Read(buffer, 0, 4096);
                MemoryStream ms = new MemoryStream(buffer);
                BinaryFormatter bf = new BinaryFormatter();
                ms.Position = 0;
                object temp = bf.Deserialize(ms);

                string reply="";
                if (temp is string)
                    reply = (string)temp;
                else
                    break;
                string[] ans = reply.Split('&');

                XmlTextWriter outp = new XmlTextWriter("output.xml", null);
                outp.Formatting = Formatting.Indented;

                outp.WriteStartDocument();
                outp.WriteStartElement("Outputs");
                foreach (string blah in ans)
                {
                    outp.WriteStartElement("Div");
                    outp.WriteStartElement("Id",blah.Split(' ')[0]);
                    outp.WriteEndElement();
                    outp.WriteStartElement("PosX", blah.Split(' ')[1]);
                    outp.WriteEndElement();
                    outp.WriteStartElement("PosY", blah.Split(' ')[2]);
                    outp.WriteEndElement();
                    outp.WriteEndElement();
                }
                outp.WriteEndElement();
                outp.WriteEndDocument();
                outp.Flush();
                outp.Close();
            }
        }

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect("127.0.0.1", 1303);
                Console.WriteLine("Connected to server");

                byte[] message;
                NetworkStream serverStream = client.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();

                int option = Int32.Parse(Console.ReadLine());
                int rb;
                
                switch (option)
                {
                    case 1:
                        message = encoder.GetBytes("1");
                        serverStream.Write(message, 0, message.Length);
                        getFile(serverStream);
                        break;
                    case 2:
                        message = encoder.GetBytes("3");
                        serverStream.Write(message, 0, message.Length);
                        message = new byte[4096];
                        rb = serverStream.Read(message, 0, 4096);
                        if (encoder.GetString(message, 0, rb).ToLower().Equals("accepted"))
                        {
                            sendData(serverStream);
                            recvData(serverStream);
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid Mode");
                        break;
                }
                client.Close();
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
