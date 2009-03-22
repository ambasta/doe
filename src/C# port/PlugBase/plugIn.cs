using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doe.PlugBase
{
    public interface plugIn
    {
        #region Methods
        void properties(String[] mat);
        int score(int[][] mat);
        #endregion
    }
}
