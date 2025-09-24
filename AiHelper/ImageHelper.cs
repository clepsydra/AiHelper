using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace AiHelper
{    internal class ImageHelper
    {
        public static byte[] CaptureImage(bool showImage)
        {
            VideoCapture capture = new VideoCapture(); //create a camera capture
            Bitmap image = capture.QueryFrame().ToBitmap();

            var bytes = ToByteArray(image);

            if (showImage)
            {
                string fileName = Path.Combine(Path.GetTempPath(),$"aiHelper{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.png");
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
    }
}
