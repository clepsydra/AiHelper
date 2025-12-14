using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AiHelper.Plugin
{
    internal class PluginRegistrar
    {
        internal static void RegisterPlugins(Kernel kernel, Action closeSession)
        {
            kernel.Plugins.AddFromType<AnalyzeImagePlugin>("AnalyzeImagePlugin");
            kernel.Plugins.AddFromType<EMailPlugin>("EMailPlugin");
            kernel.Plugins.AddFromType<ShoppingListPlugin>("ShoppingListPlugin");

            var closeSessionPlugin = new CloseHistoryPlugin(closeSession);
            kernel.Plugins.AddFromObject(closeSessionPlugin, "CloseSessionPlugin");
        }
    }
}
