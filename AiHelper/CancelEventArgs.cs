using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace AiHelper
{
    public class CancelEventArgs : EventArgs
    {
        public bool IsHandled { get; set; }
    }
}
