using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AiHelper.Plugin
{
    public class CloseHistoryPlugin
    {
        Action closeSession;

        public CloseHistoryPlugin(Action closeSession)
        {
            this.closeSession = closeSession;
        }

        [KernelFunction]
//        [Description(@"When the user asks to close the current session or end the current discussion or the current topic this function can be used.
//At the end tell the user in German that you are waiting for the code word 'Computer' to start listening.")]
        [Description(@"When the user asks to close the current session or end the current discussion or the current topic this function can be used.
At the end tell the user in German that you are waiting for the the user pressing the space bar in German 'Leertaste' to start listening.")]
        public string CloseSession()
        {
            this.closeSession();
            return "Successfully closed session";
        }
    }
}
