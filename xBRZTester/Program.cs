using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Permissions;
using xBRZNet;

namespace xBRZTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: xBRZTester.exe ScaleFactor(2-6) InputPath OutputPath");
                Console.WriteLine("\tEx:\txBRZTester.exe 4 C:\\example\\input.png output.png");
                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();

            }
            else
            {
                SaveScaledImage(Int32.Parse(args[0]), args[1], args[2]);
            }
        }

        private static void SaveScaledImage(int scaleFactor, string inputPath, string outputPath)
        {
            var scaledImage = new xBRZScaler().ScaleImage(new Bitmap(inputPath), scaleFactor);

            scaledImage.Save(outputPath);
        }
    }
}
