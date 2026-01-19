using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AiHelper.Plugin
{
    internal class QrCodeReaderPlugin
    {
        private readonly Action<string, bool> addToOutput;
        private readonly Action closeSession;
        private readonly ICancelRegistrar cancelRegistrar;
        private QRCodeReader? qrCodeReader = null;

        private bool isQrCodeReaderRunning = false;

        public QrCodeReaderPlugin(Action<string, bool> addToOutput, Action closeSession, ICancelRegistrar cancelRegistrar)
        {
            this.addToOutput = addToOutput;
            this.closeSession = closeSession;
            this.cancelRegistrar = cancelRegistrar;
        }

        [KernelFunction]
        [Description(@"Starts the QR Code Reader. That tool does then does the interaction with the user.
The function returns an information whether the QR Code reader was started successfully, or if there is a problem.
Tell the user about the answer.
After calling this function you MUST not ask the user whether he wants to do something else.")]
        public string StartQrCodeReader()
        {
            if (isQrCodeReaderRunning)
            {
                return "QR Code Reader already running";
            }

            qrCodeReader = new QRCodeReader(this.addToOutput, this.cancelRegistrar);
            closeSession();
            isQrCodeReaderRunning = true;
            Task.Run(async () =>
            {
                //await Task.Delay(8000);
                qrCodeReader.Run();
                isQrCodeReaderRunning = false;
            });

            return "QR Code Reader started";
        }

        [KernelFunction]
        [Description(@"Stops the QR Code Reader. Returns a text indicating whether stopping it was successful or not.")]
        public string StopQrCodeReader()
        {
            qrCodeReader?.Stop();
            isQrCodeReaderRunning = false;
            return "Successfully stopped";
        }
    }
}
