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
        internal static void RegisterPlugins(Kernel kernel, Action<string, bool> addToOutput, Action closeSession, ICancelRegistrar cancelRegistrar)
        {
            kernel.Plugins.AddFromType<AnalyzeImagePlugin>("AnalyzeImagePlugin");
            kernel.Plugins.AddFromType<EMailPlugin>("EMailPlugin");
            kernel.Plugins.AddFromType<ShoppingListPlugin>("ShoppingListPlugin");
            kernel.Plugins.AddFromType<DateTimePlugin>("DateTimePlugin");

            var braillePlugin = new BraillePlugin(addToOutput, closeSession);
            kernel.Plugins.AddFromObject(braillePlugin, "BraillePlugin");

            var closeSessionPlugin = new CloseHistoryPlugin(closeSession);
            kernel.Plugins.AddFromObject(closeSessionPlugin, "CloseSessionPlugin");

            var qrCodeReaderPlugin = new QrCodeReaderPlugin(addToOutput, closeSession);
            kernel.Plugins.AddFromObject(qrCodeReaderPlugin, "QrCodeReaderPlugin");

            if (ChristmasGiftPlugin.IsAvailable)
            {
                var christmasGifePlugin = new ChristmasGiftPlugin(cancelRegistrar, closeSession);
                kernel.Plugins.AddFromObject(christmasGifePlugin, "ChristmasGiftPlugin");
            }           
        }
    }
}
