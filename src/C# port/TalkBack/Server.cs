using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Reflection;
using System.Collections.Generic;

using Doe.Exceptions;
using Doe.mXMLHandler;
using Doe.PlugBase;
using Doe.Core;
using Doe;

namespace Doe.TalkBack
{
    public class Server
    {
        private TcpListener tcpListener;
        private xHandle config;

        public Server(xHandle temp)
        {
            config = temp;
            tcpListener = new TcpListener(IPAddress.Any, 1303);
        }

        public void ListenForClients()                      //function to actually listen to clients
        {
            Console.WriteLine("Listening for Clients");
            this.tcpListener.Start();
            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();      //handle for new connection
                Thread clientThread = new Thread(new ParameterizedThreadStart(wrapCore));           //pass handle to wrapCore and go back to listening
                try
                {
                    clientThread.Start(client);
                }
                catch(customException e)
                {
                    Console.WriteLine(e.ErrorMessage);
                }
            }
        }

        public void wrapCore(object clientArg)          //function to handle connection from client
        {
            TcpClient client = (TcpClient)clientArg;    //convert passed object to socket handle
            Console.WriteLine("Client connected");
            byte[] message = new byte[4096];
            NetworkStream clientStream = client.GetStream();
            int readBytes;

            try
            {                                                       //read some data from socket
                readBytes = clientStream.Read(message, 0, 4096);    //if client disconnects quit function
            }
            catch
            {
                return;
            }

            if (readBytes == 0)
            {
                Console.WriteLine("Client Disconnected");
                return;
            }

            int mode = Int32.Parse(common.code(message,0,readBytes));       //parse mode from input
            try
            {       //if any of the called function crash due to abrupt client disconnection.. print that client disconnected and leave
                switch (mode)
                {
                    case 1:
                        sendXml(clientStream);      //return xml
                        break;
                    case 3:
                        message = common.code("Accepted");         //return accepted
                        clientStream.Write(message, 0, message.Length);
                        clientStream.Flush();
                        block(clientStream);                            //call block to process input
                        break;
                    default:
                        message = common.code("Invalid Mode");     //return invalid mode
                        clientStream.Write(message, 0, message.Length);
                        clientStream.Flush();
                        break;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {   //try to close clientstream
                clientStream.Close();
            }
            catch { }
            client.Close();
        }

        private void sendXml(NetworkStream clientStream)            //send xml file to client
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream("Config.xml", FileMode.Open, FileAccess.Read);
                UInt64 fLen = (UInt64)fs.Length;        //store file size of config

                clientStream.Write(BitConverter.GetBytes(fLen), 0, 8);      //write first 8 bytes as file size
                clientStream.Flush();

                int br = 0;
                byte[] buff = new byte[4096];

                do
                {
                    br = fs.Read(buff, 0, 4096);
                    clientStream.Write(buff, 0, br);
                    clientStream.Flush();
                } while (br == 4096);
            }
            catch (Exception e)
            {
                try
                {
                    fs.Close();
                }
                catch { }
                Console.WriteLine(e.Message);
            }
        }

        private void block(NetworkStream clientStream)
        {
            byte[] message = new byte[4096];
            String[] data;          //input from client as divCount,{div$divid},{plugin$pluginName${pluginAttib}}
            List<int> divs = new List<int>();                       //list of divisions
            List<plugIn> plugsStZ = new List<plugIn>();             //list of plugins for stage 0
            List<plugIn> plugsStO = new List<plugIn>();             //list of plugins for stage 1
            List<string> queue = new List<string>();                //shared queue for output

            int readBytes;
            readBytes = clientStream.Read(message, 0, 4096);        //recieve input from client

            if (readBytes == 0)
            {
                Console.WriteLine("Client Disconnected");
                return;
            }

            object temp = common.deserialise(message);                     //deserialize input

            if (!(temp is String[]))                                //if input isn't a string array
            {
                message = common.code("Invalid Data");
                clientStream.Write(message, 0, message.Length);
                clientStream.Flush();
                return;
            }

            data = (String[])temp;                                  //convert deserialized input to string array
            int dCount = 0;                                         //no. of divisions
            foreach(String temper in data)
            {
                if (!temper.Equals(data[0]))
                {
                    String[] parsed = temper.Split('$');            //parse data element
                    if (parsed[0].ToLower().Equals("plugin"))
                    {
                        if (!config.checkPlug(parsed[1]))           //check if desired plugin exists
                        {
                            message = common.code("Invalid Plugin: " + parsed[1]);
                            clientStream.Write(message, 0, message.Length);
                            clientStream.Flush();
                            return;
                        }

                        String[] plugDet = config.getPlug(parsed[1]);       //get details of desired plugin
                        int stage = Int32.Parse(plugDet[4]);                //get stage of desired plugin
                        plugIn templug = createPlug(plugDet);               //actually create the plugin
                        if (templug == null)                                //if plugin couldn't be created return error
                        {
                            Console.WriteLine("Failed to load plugin" + plugDet[0]);
                            Environment.Exit(1);    //bugged plugin in config, exit the server
                        }

                        templug.properties(parsed[2].Split('*'),dCount);    //set plugin properties with parsed attributes specified as {{att }[-" "]*}[-"*"] and the number of divs
                        switch (stage)      //add initialized plugin to respective queue
                        {
                            case 0:
                                plugsStZ.Add(templug);
                                break;
                            case 1:
                                plugsStO.Add(templug);
                                break;
                        }
                    }
                    else if (parsed[0].ToLower().Equals("div"))     //if the parsed data indicates div
                    {                                               //add divId to divList
                        divs.Add(Int32.Parse(parsed[1]));
                    }
                    else
                    {       //if neither div not plugin is specified, return error Message
                        message = common.code("Invalid Mode");
                        clientStream.Write(message, 0, message.Length);
                        clientStream.Flush();
                        return;
                    }
                }
                else
                {       //for the first element of data, find out divCount from it
                    dCount = Int32.Parse(data[0]);
                }
            }

            //spawn a coreprocess and ask to write to queue
            CoreProcess processor = new CoreProcess(divs, plugsStZ, queue);
            Thread processT = new Thread(new ThreadStart(processor.permute));
            processT.Start();
            //spwan a coreoutput and ask to read from queue
            CoreOutput display = new CoreOutput(queue, clientStream);
            Thread displayT = new Thread(new ThreadStart(display.show));
            displayT.Start();

            Thread.Sleep(100);
            while (displayT.IsAlive) ;  //sping till i am pushing more output
            if(processT.IsAlive)
                processT.Abort();           //as display is over due to either disconnect or no data, simply ask processT to abort if needed
            
            processT.Join();
            displayT.Join();
        }

        private plugIn createPlug(string[] data)
        {
            if(File.Exists(data[1]))
            {
                Assembly assembly = Assembly.LoadFile(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + data[1]);
                foreach (Type type in assembly.GetTypes())
                {
                    object test = Activator.CreateInstance(type);
                    if (test is plugIn)
                        return (plugIn)test;
                }
            }
            return null;
        }
    }
}
