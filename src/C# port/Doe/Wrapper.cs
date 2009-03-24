using System;
using System.Threading;

using Doe.Exceptions;
using Doe.mXMLHandler;
using Doe.TalkBack;

namespace Doe
{
    class Wrapper                                                   //class to load and validate XML
    {                                                               //also starts the server
        static void Main(string[] args)
        {
            String docPath, scmPath;
            docPath = "Config.xml";
            scmPath = "Config.xsd";
            xHandle config = new xHandle(docPath, scmPath);
            try
            {
                if (!config.verify())
                {
                    Console.WriteLine("Bad config file");
                    Environment.Exit(1);
                }
                Console.WriteLine("Successfully loaded xml");
            }
            catch (customException e)
            {
                Console.WriteLine(e.ErrorMessage);
                Environment.Exit(1);
            }

            Server myServer = new Server(config);
            Thread myThread = new Thread(new ThreadStart(myServer.ListenForClients));
            myThread.Start();

            while (!myThread.IsAlive) ;         //spin a while for thread to be alive
            while (myThread.IsAlive)
                Thread.Sleep(100);          //do nothing while thread is alive
            myThread.Join();
        }
    }
}
