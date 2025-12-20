using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiHelper
{
    public interface ICancelRegistrar
    {
        event EventHandler<CancelEventArgs> Cancel;
    }
}
