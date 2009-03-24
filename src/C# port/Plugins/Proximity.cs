using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Doe.PlugBase;

namespace Plugins
{
    public class Proximity : plugIn
    {
        private int[,] weight;
        private int[,] proximity;
        int divSize;

        public void properties(string[] mat,int dSize)
        {
            divSize = dSize;
            weight = new int[dSize, dSize];
            proximity = new int[dSize, dSize];
            for (int i = 0; i < dSize; i++)
            {
                for (int j = 0; j < dSize; j++)
                {
                    weight[i, j] = 0;
                    proximity[i, j] = 0;
                }
            }
            foreach (string temp in mat)
            {
                string[] tArr = temp.Split(' ');
                //from to weight prox
                int from = Int32.Parse(tArr[0]);
                int to = Int32.Parse(tArr[1]);
                int wt = Int32.Parse(tArr[2]);
                int prox = (Convert.ToBoolean(tArr[3])) ? 1 : -1;
                weight[from, to] = wt;
                proximity[from, to] = prox;
            }
        }

        public double score(int[,] mat)
        {
            double score = 0;
            for (int i = 0; i < divSize; i++)
                for (int j = 0; j < divSize; j++)
                    score += distance(mat, i, j) * weight[i, j] * proximity[i, j];
            return score;
        }

        private double distance(int[,] mat, int i, int j)
        {
            double x2 = mat[i, 0] - mat[j, 0];
            x2 *= x2;
            double y2 = mat[i, 1] - mat[j, 1];
            y2 *= y2;
            double sum = x2 + y2;
            return Math.Sqrt(sum);
        }
    }
}
