using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
            while (true)
            {
                if (queue.Count == 0)
                {
                    break;
                }
                string ans = queue[0];
                queue.Remove(ans);

                MemoryStream ms = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms,(object)ans);
                byte[] reply = ms.ToArray();

                clientStream.Write(reply, 0, reply.Length);
            }
        }
    }
}
