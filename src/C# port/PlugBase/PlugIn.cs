﻿namespace Doe.PlugBase
{
    public interface plugIn
    {
        #region Methods
        void properties(string[] mat, int dSize);
        double score(int[,] mat);
        #endregion
    }
}
