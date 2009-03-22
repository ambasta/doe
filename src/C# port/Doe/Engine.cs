using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Xml;

namespace Doe.Engine
{
    class Server
    {
        private TcpListener tcpListener;

        private void HandleClientComm(Object client)
        {
            Console.WriteLine("Server: Client Connected");
            TcpClient tcpclient = (TcpClient)client;

            byte[] message = new byte[4096];
            NetworkStream clientStream = tcpclient.GetStream();
            while (true)
            {
                int readbytes;

                try
                {
                    readbytes = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (readbytes == 0)
                {
                    System.Console.WriteLine("Server: Client Disconnected");
                    break;
                }

                ASCIIEncoding encode = new ASCIIEncoding();
                Console.WriteLine("Server: Read " + readbytes + " bytes. Message: " + encode.GetString(message, 0, readbytes));
                clientStream.Write(message, 0, readbytes);
                clientStream.Flush();
                if (encode.GetString(message, 0, readbytes).Equals("End"))
                    break;
            }
            tcpclient.Close();
        }

        public void ListenForClients()
        {
            this.tcpListener.Start();
            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        public Server()
        {
            tcpListener = new TcpListener(IPAddress.Any, 3000);
        }

    }

    class Client
    {
        IPEndPoint serverEndPoint;
        TcpClient client;

        public Client()
        {
            serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
        }

        public void connect()
        {
            client = new TcpClient();
            try
            {
                client.Connect(serverEndPoint);
                Console.WriteLine("Client: Connected to server");
            }
            catch
            {
                Console.WriteLine("Client: Connection failed");
                Environment.Exit(0);
            }

            NetworkStream clientStream = client.GetStream();
            while (true)
            {

                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes(Console.ReadLine());

                clientStream.Write(buffer, 0, buffer.Length);
                buffer = new byte[4096];
                int blen = clientStream.Read(buffer, 0, 4096);
                Console.WriteLine("Client: Read " + blen + " bytes. Message: " + encoder.GetString(buffer, 0, blen));
                clientStream.Flush();
                if (encoder.GetString(buffer, 0, blen).Equals("end"))
                {
                    Console.WriteLine("Quitting");
                    break;
                }
            }
            client.Close();
        }
    }

    class Doe
    {

        public static void Main(String[] args)
        {
            /*Thread myThread;
            int val = Int32.Parse(Console.ReadLine());
            if (val == 1)
            {
                Server myserver = new Server();
                myThread = new Thread(new ThreadStart(myserver.ListenForClients));
            }
            else
            {
                Client myclient = new Client();
                myThread = new Thread(new ThreadStart(myclient.connect));
            }

            myThread.Start();*/
        }
    }
}