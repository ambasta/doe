using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Doe.Exceptions;
using Doe.mXMLHandler;
using Doe.PlugBase;
using Doe.Core;

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

        public void ListenForClients()
        {
            this.tcpListener.Start();
            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(wrapCore));
                try
                {
                    clientThread.Start(client);
                }
                catch(customException e)
                {
                    Console.WriteLine(e.ErrorMessage);
                }
                client.Close();
            }
        }

        public void wrapCore(object clientArg)
        {
            TcpClient client = (TcpClient)clientArg;
            Console.WriteLine("Client connected");
            byte[] message = new byte[4096];
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();

            while (true)
            {
                int readBytes;
                try
                {
                    readBytes = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (readBytes == 0)
                {
                    throw new customException("Client Disconnected");
                }

                int mode = Int32.Parse(encoder.GetString(message, 0, readBytes));
                switch (mode)
                {
                    case 3:
                        block(clientStream);
                        break;
                    default:
                        message = encoder.GetBytes("Invalid Mode");
                        clientStream.Write(message, 0, message.Length);
                        clientStream.Close();
                        break;
                }
            }
        }

        private int block(NetworkStream clientStream)
        {
            byte[] message = new byte[4096];
            ASCIIEncoding encoder = new ASCIIEncoding();
            String[] data;
            List<int> divs = new List<int>();
            List<plugIn> plugsStZ = new List<plugIn>();
            List<plugIn> plugsStO = new List<plugIn>();
            List<string> queue = new List<string>();

            while (true)
            {
                int readBytes;
                try
                {
                    readBytes = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (readBytes == 0)
                {
                    throw new customException("Client Disconnected");
                }

                object temp = deserialize(message);

                if (!(temp is String[]))
                {
                    message = encoder.GetBytes("Invalid Data");
                    clientStream.Write(message, 0, message.Length);
                    clientStream.Close();
                    break;
                }

                data = (String[])temp;
                foreach(String temper in data)
                {
                    String[] parsed = temper.Split('$');
                    if (parsed[0].ToLower().Equals("plugin"))
                    {
                        if (!config.checkPlug(parsed[1]))
                        {
                            message = encoder.GetBytes("Invalid Plugin: " + parsed[1]);
                            clientStream.Write(message, 0, message.Length);
                            clientStream.Close();
                            return 1;
                        }
                        String[] plugDet = config.getPlug(parsed[1]);
                        int stage = Int32.Parse(plugDet[4]);
                        plugIn templug = createPlug(plugDet);
                        if (templug == null)
                        {
                            Console.WriteLine("Failed to load plugin" + plugDet[0]);
                            Environment.Exit(1);
                        }
                        templug.properties(parsed[2].Split('*'));
                        switch (stage)
                        {
                            case 0:
                                plugsStZ.Add(templug);
                                break;
                            case 1:
                                plugsStO.Add(templug);
                                break;
                        }
                    }
                    else if (parsed[0].ToLower().Equals("div"))
                    {
                        divs.Add(Int32.Parse(parsed[1]));
                    }
                    else
                    {
                        message = encoder.GetBytes("Invalid Mode");
                        clientStream.Write(message, 0, message.Length);
                        clientStream.Close();
                        return 1;
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
            }
            return 0;
        }

        private plugIn createPlug(String[] data)
        {
            ObjectHandle handle = Activator.CreateInstance(data[1], data[2]);
            object test = handle.Unwrap();

            if (test is plugIn)
                return (plugIn)test;
            return null;
        }

        private object deserialize(byte[] array)
        {
            MemoryStream ms = new MemoryStream(array);
            BinaryFormatter bf = new BinaryFormatter();
            ms.Position = 0;
            return bf.Deserialize(ms);
        }
    }
}
