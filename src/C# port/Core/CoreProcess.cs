using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Doe.PlugBase;

namespace Doe.Core
{
    public class CoreProcess
    {
        int[,] mat;
        List<plugIn> plugList;
        List<string> queue;

        public CoreProcess(List<int> divs, List<plugIn> recvLst, List<string> q)
        {
            mat = new int[divs.Count, 2];
            double N = (double)divs.Count;
            for (int i = 0; i < divs.Count; i++)
            {
                mat[i,0] = (int)(N* Math.Cos(2*i*Math.PI/N));
                mat[i,1] = (int)(N* Math.Sin(2*i*Math.PI/N));
            }

            plugList = recvLst;
            queue = q;
        }

        public void permute()
        {
        }
    }
}
