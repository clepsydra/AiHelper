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
            //await Task.Delay(1000);
            await Speaker2.SayAndCache(@"Ich nehme jetzt ein Bild auf.
Drei.
Zwei.
Eins.
Aufnahme.", true);
            //await Task.Delay(1000);
            //await Speaker2.Say("Drei", true);
            //await Task.Delay(1000);
            //await Speaker2.Say("Zwei", true);
            //await Task.Delay(1000);
            //await Speaker2.Say("Eins", true);
            //await Task.Delay(1000);
            //await Speaker2.Say("Aufnahme läuft", true);

            var data = CaptureImage(showImage);
            await Speaker2.SayAndCache("Bild aufgenommen, Ich analysiere jetzt das Bild");

            BinaryData binaryData = BinaryData.FromBytes(data);

            return binaryData;
        }
    }
}
