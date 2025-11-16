using System.Diagnostics;
using System.Drawing;
using System.IO;
using Emgu.CV;

namespace AiHelper
{
    internal class ImageCapture
    {
        public static byte[] CaptureImage(bool showImage)
        {
            VideoCapture capture = new VideoCapture(); //create a camera capture
            Bitmap image = capture.QueryFrame().ToBitmap();

            var bytes = ToByteArray(image);

            if (showImage)
            {
                string fileName = Path.Combine(Path.GetTempPath(), $"aiHelper{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.png");
                image.Save(fileName);
                Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
            }

            return bytes;
        }

        public static byte[] ToByteArray(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static async Task<BinaryData> CaptureImageWithHeadsup(bool showImage)
        {
            await Task.Delay(1000);
            await Speaker.Say("Halte den Gegenstand vor die Kamera");
            await Task.Delay(1000);
            await Speaker.Say("Drei");
            await Task.Delay(1000);
            await Speaker.Say("Zwei");
            await Task.Delay(1000);
            await Speaker.Say("Eins");
            await Task.Delay(1000);
            await Speaker.Say("Aufnahme läuft");

            var data = CaptureImage(showImage);
            await Speaker.Say("Bild aufgenommen, Ich analysiere jetzt das Bild");

            BinaryData binaryData = BinaryData.FromBytes(data);

            return binaryData;
        }
    }
}
