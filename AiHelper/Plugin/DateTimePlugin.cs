using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AiHelper.Plugin
{
    internal class DateTimePlugin
    {
        [KernelFunction]
        [Description(@"Returns the current date and time.")]
        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MMMM-dd HH:mm:ss");
        }
    }
}
