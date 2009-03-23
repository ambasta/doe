using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fawn
{
    public class except : Exception
    {
        public string ErrorMessage
        {
            get
            {
                return base.Message.ToString();
            }
        }

        public except(string errorMessage) : base(errorMessage) { }

        public except(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }

    public class customException : except
    {
        public customException(string errorMessage) : base(errorMessage) { }

        public customException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
}
