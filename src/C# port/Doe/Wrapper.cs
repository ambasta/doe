using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Doe.Exceptions;
using Doe.mXMLHandler;
using Doe.TalkBack;

namespace Doe
{
    class Wrapper
    {
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
        }
    }
}
