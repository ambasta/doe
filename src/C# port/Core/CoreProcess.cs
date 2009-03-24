using System;
using System.Collections.Generic;
using System.Threading;

using Doe.PlugBase;

namespace Doe.Core
{
    public class CoreProcess
    {
        private int[,] mat;
        private int mlen;
        private List<plugIn> plugList;
        private List<string> queue;

        public CoreProcess(List<int> divs, List<plugIn> recvLst, List<string> q)
        {
            mat = new int[divs.Count, 2];
            mlen = divs.Count;
            int N = (int)divs.Count;
            for (int i = 0; i < divs.Count; i++)
            {
                mat[i,0] = (int)(N* Math.Cos(2*i*Math.PI/N));
                mat[i,1] = (int)(N* Math.Sin(2*i*Math.PI/N));
            }

            plugList = recvLst;
            queue = q;
            queue.Add("Null");
        }

        public void permute()
        {
            int maxMoves = 1;
            double matScore = score();
            bool noMovesOptimize = true;

            while (true)
            {
                noMovesOptimize = true;
                if (maxMoves > mlen)
                    break;
                for(int i=0; i<mlen; i++)
                {
                    int movex=0,movey=0;
                    double initScore = matScore;
                    for (int xp = maxMoves*-1; xp <= maxMoves; xp++)
                    {
                        for (int yp = maxMoves*-1; yp <= maxMoves; yp++)
                        {
                            mat[i, 0] += xp;
                            mat[i, 1] += yp;

                            if (check(i))
                            {
                                double tscore = score();
                                if (initScore < tscore)
                                {
                                    initScore = tscore;
                                    movex = xp;
                                    movey = yp;
                                    //write();
                                }
                            }

                            mat[i, 0] -= xp;
                            mat[i, 1] -= yp;
                        }
                    }
                    if (initScore < matScore)
                    {
                        matScore = initScore;
                        mat[i, 0] += movex;
                        mat[i, 1] += movey;
                        noMovesOptimize = false;
                        write();
                    }
                }
                if (noMovesOptimize)
                    maxMoves++;
            }
            while (queue.Count > 10) 
                Thread.Sleep(100);       //loop till all other outputs have been published
            queue.Remove(queue[0]);         //empty queue to indicate phase I is over
        }

        private bool check(int x)
        {
            for (int i = 0; i < mlen; i++)
            {
                if ((mat[i,0] == mat[x,0]) && (mat[i,1] == mat[x,1]) && (i != x))
                    return false;
            }
            return true;
        }

        private double score()
        {
            double tscore = 0;
            foreach (plugIn temp in plugList)
            {
                tscore += temp.score(mat);
            }
            return tscore;
        }

        private void write()
        {
            string temp = "";
            for (int i = 0; i < mlen; i++)
            {
                temp += Convert.ToString(i)+" " +Convert.ToString(mat[i,0])+" "+Convert.ToString(mat[i,1])+"&";
            }
            while (queue.Count > 10) ;
            queue.Add(temp.Substring(0,temp.Length-1));
        }
    }
}
