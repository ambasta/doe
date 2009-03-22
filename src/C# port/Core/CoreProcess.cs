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
            int N = (int)divs.Count;
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
            int sjump = 1;
            float scores = score();
            int cond = 5;
            while (true)
            {
                cond = 5;
                if (sjump <= mat.Length)
                {
                    for(int i=0; i<mat.Length; i++)
                    {
                        for (int xp = 0; xp <= sjump; xp++)
                        {
                            for (int yp = 0; yp <= sjump; yp++)
                            {
                                mat[i,0]+=xp;
                                mat[i,1]+=yp;
                                if(check(i))
                                {
                                    float tscore = score();
                                    if(scores < tscore)
                                    {
                                        scores = tscore;
                                        write();
                                        cond = 0;
                                    }
                                }
                                if(cond==0)
                                    break;
                            }
                            if(cond==0)
                                break;
                        }
                    }
                }
                else
                    break;
                if (cond == 5)
                    sjump++;
                else
                    sjump = 1;
            }
        }

        private bool check(int x)
        {
            for (int i = 0; i < mat.Length; i++)
            {
                if ((mat[i,0] == mat[x,0]) && (mat[i,1] == mat[x,1]) && (i != x))
                    return false;
            }
            return true;
        }

        private float score()
        {
            float tscore = 0;
            foreach (plugIn temp in plugList)
            {
                tscore += temp.score(mat);
            }
            return tscore;
        }

        private void write()
        {
            string temp = "";
            for (int i = 0; i < mat.Length; i++)
            {
                temp += Convert.ToString(i)+" " +Convert.ToString(mat[i,0])+" "+Convert.ToString(mat[i,1])+"&";
            }
            queue.Add(temp);
        }
    }
}
