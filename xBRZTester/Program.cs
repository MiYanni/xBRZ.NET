using System.Drawing;
using System.Drawing.Imaging;
using xBRZNet;

namespace xBRZTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SaveScaledImage();
        }

        private static void SaveScaledImage()
        {
            var originalImage = new Bitmap(@"..\..\Images\Chrono Trigger2.png");

            const string fileName = "Image";
            const string imageExtension = ".png";

            originalImage.Save(fileName + "-orig" + imageExtension, ImageFormat.Png);

            const int scaleSize = 3;
            var scaledImage = new xBRZScaler().ScaleImage(originalImage, scaleSize);

            scaledImage.Save(fileName + "-" + scaleSize + "xBRZ" + imageExtension, ImageFormat.Png);
        }
    }
}
