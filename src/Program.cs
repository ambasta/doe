using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace doe
{
    class Server
    {
        private TcpListener tcpListener;

        private void HandleClientComm(Object client)
        {
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
                    System.Console.WriteLine("Client Disconnected");
                    break;
                }

                ASCIIEncoding encode = new ASCIIEncoding();
                System.Console.WriteLine(encode.GetString(message, 0, readbytes));
                clientStream.Write(message, 0, message.Length);
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
                clientThread.Start();
            }
        }

        public Server()
        {
            tcpListener = new TcpListener(IPAddress.Any, 3000);            
        }

    }

    class Client
    {
        public Client()
        {
            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);

            client.Connect(serverEndPoint);

            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes("Hello Server");

            clientStream.Write(buffer, 0, buffer.Length);
            while (true)
            {
                buffer = new byte[4096];
                int blen;

                try
                {
                    blen = clientStream.Read(buffer, 0, 4096);
                }
                catch
                {
                    Console.WriteLine("Error while reading from server");
                }

                Console.WriteLine(encoder.GetString(buffer));
                clientStream.Flush();
            }
        }
    }

    class Program
    {
        public static void Main(String[] args)
        {
            int choice;
            choice = Console.Read()-'0';
            Console.WriteLine(choice);
            if (choice == 1)
            {
                Server myserver = new Server();
                myserver.ListenForClients();
            }
            else
            {
                Client myclient = new Client();
            }
        }
    }
}
