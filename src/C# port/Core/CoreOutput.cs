using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

using Doe;

namespace Doe.Core
{
    public class CoreOutput
    {
        List<string> queue;
        NetworkStream clientStream;

        public CoreOutput(List<string> q, NetworkStream cs)
        {
            queue = q;
            clientStream = cs;
        }

        public void show()
        {
            byte[] reply = new byte[4096];

            while (true)
            {
                if (queue.Count == 0)       //if queue is empty .. then phase I is over.. indicate and quit loop
                {
                    reply = common.serialize("Done");

                    clientStream.Write(reply, 0, reply.Length);
                    clientStream.Flush();
                    break;
                }
                else if (queue.Count == 1)  //if there is only 1 element, we're waiting for more output or phase I to be over
                {
                    Thread.Sleep(100);
                    continue;
                }

                string ans = queue[1];              //otherwise take element from queue serialize it and
                reply = common.serialize(ans);      //send it to client and remove element from queue

                clientStream.Write(reply, 0, reply.Length);
                clientStream.Flush();
                queue.Remove(ans);

                reply = new byte[4096];                         //after pushing output wait for acceptance
                int rb = clientStream.Read(reply, 0, 4096);
                if (rb == 0)                                    //rb = 0 -> client disconnected
                    return;                                     //hence leave thread

                if (common.code(reply, 0, rb).Equals("Accepted"))
                {
                    continue;
                }
                else
                {
                    return;                                     //Client didn't say accepted after recieving output Map, so return
                }
            }
        }
    }
}
