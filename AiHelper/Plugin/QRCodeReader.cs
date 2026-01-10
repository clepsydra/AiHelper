using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace AiHelper.Plugin
{
    internal class QRCodeReader
    {
        private readonly Action<string, bool> addToOutput;        

        private bool endRequested = false;

        public QRCodeReader(Action<string, bool> addToOutput)
        {
            this.addToOutput = addToOutput;
        }

        internal async void Run()
        {
            using var capture = new VideoCapture();
            var qrCodeDetector = new QRCodeDetector();

            string previousText = string.Empty;
            DateTime previousDetectedAt = DateTime.MinValue;

            DateTime lastQrCodeDetectedAt = DateTime.Now;
            bool timedOut = false;

            while (!endRequested)
            {
                if (DateTime.Now.Subtract(lastQrCodeDetectedAt).TotalMinutes > 3)
                {
                    endRequested = true;
                    timedOut = true;
                }

                using Mat frame = capture.QueryFrame();

                if (frame == null || frame.IsEmpty)
                    return;

                // Validate frame has proper dimensions and channels
                if (frame.Width <= 0 || frame.Height <= 0 || frame.NumberOfChannels == 0)
                    return;

                // Clone the frame to ensure it has proper memory layout and type
                using Mat processFrame = frame.Clone();

                //CameraImage.Source = ConvertMatToBitmapSource(processFrame);

                // Convert to grayscale for QR detection
                using Mat grayFrame = new Mat(processFrame.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                CvInvoke.CvtColor(processFrame, grayFrame, ColorConversion.Bgr2Gray);

                VectorOfPointF corners = new VectorOfPointF();
                qrCodeDetector.Detect(processFrame, corners);
                if (corners.Length == 0)
                {
                    continue;
                }

                // QR-Codes erkennen
                using VectorOfCvString decodedInfo = new VectorOfCvString();
                var textContent = qrCodeDetector.Decode(processFrame, corners);
                if (string.IsNullOrWhiteSpace(textContent))
                {
                    continue;
                }

                lastQrCodeDetectedAt = DateTime.Now;

                if (textContent.Equals(previousText, StringComparison.OrdinalIgnoreCase)
                    && DateTime.Now.Subtract(previousDetectedAt).TotalSeconds < 2)
                {
                    continue;
                }

                await Speaker2.SayAndCache(textContent, true);

                previousText = textContent;
                previousDetectedAt = DateTime.Now;
                lastQrCodeDetectedAt = DateTime.Now;
            }

            if (timedOut)
            {
                await SayAndCache("Es wurden länger keine QR Codes gelesen. Der QR Code Reader wurde daher beendet.");
            }            
        }

        private async Task SayAndCache(string message)
        {
            addToOutput(message, false);
            await Speaker2.SayAndCache(message, true);
        }

        internal void Stop()
        {
            this.endRequested = true;
        }
    }
}
