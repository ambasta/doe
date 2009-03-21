using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace doe
{
    interface PluginFace
    {
        private int[][] propMat;
        public String register();
        public void PluginFace(int[][] properties);
        public int score(int[][] graph);
    }
}
