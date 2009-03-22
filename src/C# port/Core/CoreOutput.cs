using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

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
        }
    }
}
