using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDM_comm
{
    public class SDM_ErrException : Exception
    {
        public SDM_ErrException(string message) : base(message) { }
    }
}
