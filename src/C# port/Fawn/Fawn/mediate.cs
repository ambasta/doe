using System;
using System.IO;
using System.Xml;
using System.Net.Sockets;
using System.Xml.Schema;

using Doe;


namespace Fawn
{
    class mediate
    {
        static void getFile(NetworkStream ns)
        {
            UInt64 fLen;
            int readBytes = 0;
            byte[] message = new byte[8];
            FileStream fs;

            try
            {
                fs = new FileStream("Config.xml", FileMode.Create, FileAccess.Write);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

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
                outData[i] = "Div$" + divId.InnerText;
                i++;
            }
                //each element contains div$divid

            nodes = input.getNodes("/Inputs/Plugin");
            foreach (XmlNode node in nodes)
            {
                //each element contains plugin$pluginName$Args*Args*Args..
                XmlElement plugId = (XmlElement)node.ChildNodes[0];
                outData[i] = "Plugin$" + plugId.InnerText + "$";
                for (int inputs = 0; inputs < node.ChildNodes.Count - 1; inputs++)
                {
                        plugId = (XmlElement)node.ChildNodes[inputs + 1];
                        outData[i] += plugId.InnerText + "*";
                }
                outData[i] = outData[i].Substring(0, outData[i].Length - 1);
                i++;
            }

            byte[] reply = common.serialize(outData);

            ns.Write(reply, 0, reply.Length);
        }

        static void recvData(NetworkStream ns)      //function to read output from server on data
        {
            while (true)
            {
                byte[] buffer = new byte[4096];
                int rb = ns.Read(buffer, 0, 4096);
                if (rb == 0)
                {
                    Console.WriteLine("Server Disconnected");
                    return;
                }

                object temp = null;
                try
                {
                    temp = common.deserialise(buffer);      //deserialise output
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

                string reply = "";
                if (temp is string)                         //if deserialized to string then continue else return
                    reply = (string)temp;
                else
                    return;

                if (temp.Equals("Done"))                    //if server says its done.. return
                {
                    Console.WriteLine("Data Processed");
                    return;
                }
                else
                {
                    string[] ans = reply.Split('&');
                    XmlTextWriter outp;

                    try
                    {
                        outp = new XmlTextWriter("output.xml", null);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return;
                    }

                    outp.Formatting = Formatting.Indented;

                    outp.WriteStartDocument();
                    outp.WriteStartElement("Outputs");
                    foreach (string blah in ans)
                    {
                        outp.WriteStartElement("Div");
                        outp.WriteStartElement("Id", blah.Split(' ')[0]);
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

                    buffer = common.code("Accepted");
                    ns.Write(buffer, 0, buffer.Length);
                }
            }
        }

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect("127.0.0.1", 1303);                  //Connect to server
                Console.WriteLine("Connected to server");

                byte[] message;
                NetworkStream serverStream = client.GetStream();

                int option = Int32.Parse(Console.ReadLine());       //Get mode from user
                int rb;
                
                switch (option)
                {
                    case 1:
                        message = common.code("1");
                        serverStream.Write(message, 0, message.Length);
                        getFile(serverStream);
                        break;
                    case 2:
                        message = common.code("3");
                        serverStream.Write(message, 0, message.Length);
                        message = new byte[4096];
                        rb = serverStream.Read(message, 0, 4096);
                        if (common.code(message, 0, rb).ToLower().Equals("accepted"))
                        {
                            sendData(serverStream);
                            recvData(serverStream);
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid Mode");
                        break;
                }
                try
                {
                    serverStream.Close();
                }
                catch { }
                client.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
