using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace doe.PlugBase
{
    public class nullPlug : plugIn
    {
        public String[,] info()
        {
            String[,] nullBase = new String[,]{{"Version","0.1"},{"Name","Null Plugin"},{"Description","A plugin that does nothing"},{"Phase","1"}};
            String[,] input = new String[,] { { "div", "from" }, { "div", "to" }, { "zinteger", "weight" } , { "boolean", "type" } };
            String[,] inpuz = new String[,] { { "div", "Area of" }, { "pinteger", "Length" }, { "pinteger", "Width" } };
            return nullBase;
        }

        public void properties(String[] mat)
        {
        }

        public int score(int[][] graph)
        {
            return 0;
        }
    }
}
