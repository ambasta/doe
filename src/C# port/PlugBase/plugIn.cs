using System;
using System.Collections.Generic;
using System.Text;

namespace Doe.PlugBase
{
    public interface plugIn
    {
        #region Methods
        String[,] info();                   //passes info on the plugin like version, name, type etc etc
        void properties(String[] mat);
        int score(int[][] mat);
        #endregion
    }
}