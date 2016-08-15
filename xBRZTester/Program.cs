using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using xBRZ.NET;

namespace xBRZTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //SaveScaledImage();
            //var result = new int[9];
            //Enumerable.Range(0, 9).ToList().ForEach(x =>
            //{
            //    var nextIndex = (x == 0 || x + 3 > 8) ? 
            //} );
            var matrix = Enumerable.Range(0, 9).ToArray();
            matrix.ToList().ForEach(Console.Write);
            Console.WriteLine();
            //RotateSquare1DMatrixClockwise(matrix).ToList().ForEach(Console.Write);
            matrix = Rotate1DSquareMatrixClockwise(matrix);
            matrix.ToList().ForEach(Console.Write);
            Console.WriteLine();
            matrix = Rotate1DSquareMatrixClockwise(matrix);
            matrix.ToList().ForEach(Console.Write);
            Console.WriteLine();
            matrix = Rotate1DSquareMatrixClockwise(matrix);
            matrix.ToList().ForEach(Console.Write);
            Console.WriteLine();
            matrix = Rotate1DSquareMatrixClockwise(matrix);
            matrix.ToList().ForEach(Console.Write);

            //var matrix2d = new[,] {{0, 1, 2}, {3, 4, 5}, {6, 7, 8}};
            //TransposeSquareMatrixInPlace(matrix2d);
            //var rotated = RotateMatrix(matrix2d, 3);
            Console.ReadKey();
        }

        private static int[] RotateSquare1DMatrixClockwise(int[] matrix)
        {
            var size = (int)Math.Sqrt(matrix.Length);

            for (var n = 0; n < (size - 1); ++n)
            {
                for (var m = n + 1; m < size; ++m)
                {
                    var temp = matrix[n * size + m];
                    matrix[n * size + m] = matrix[m * size + n];
                    matrix[m * size + n] = temp;
                }
            }
            return matrix;
        }

        //http://stackoverflow.com/a/16981138/294804
        public static void TransposeSquareMatrixInPlace(int[,] matrix)
        {
            if (matrix == null) throw new ArgumentNullException("matrix");
            if (matrix.GetLength(0) != matrix.GetLength(1)) throw new ArgumentOutOfRangeException("matrix", "matrix is not square");

            int size = matrix.GetLength(0);

            for (int n = 0; n < (size - 1); ++n)
            {
                for (int m = n + 1; m < size; ++m)
                {
                    int temp = matrix[n, m];
                    matrix[n, m] = matrix[m, n];
                    matrix[m, n] = temp;
                }
            }
        }

        //http://stackoverflow.com/a/42535/294804
        public static int[,] RotateMatrix(int[,] matrix, int n)
        {
            int[,] ret = new int[n, n];

            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    ret[i, j] = matrix[n - j - 1, i];
                }
            }

            return ret;
        }

        public static int[] Rotate1DSquareMatrixClockwise(int[] matrix)
        {
            var size = (int)Math.Sqrt(matrix.Length);
            var result = new int[matrix.Length];

            for (var i = 0; i < size; ++i)
            {
                for (var j = 0; j < size; ++j)
                {
                    result[i * size + j] = matrix[(size - j - 1) * size + i];
                }
            }

            return result;
        }

        private static void SaveScaledImage()
        {
            var originalImage = new Bitmap(@"..\..\Images\Chrono Trigger2.png");

            // Specify a pixel format.
            const PixelFormat pxf = PixelFormat.Format32bppRgb;

            //http://stackoverflow.com/a/2016509/294804
            var fixedFormatImage = new Bitmap(originalImage.Width, originalImage.Height, pxf);
            using (var gr = Graphics.FromImage(fixedFormatImage))
            {
                gr.DrawImage(originalImage, new Rectangle(0, 0, fixedFormatImage.Width, fixedFormatImage.Height));
            }

            //https://msdn.microsoft.com/en-us/library/ms229672(v=vs.90).aspx
            const string fileName = "Image";
            const string imageExtension = ".png";

            fixedFormatImage.Save(fileName + "-orig" + imageExtension, ImageFormat.Png);

            // Lock the bitmap's bits.
            var rect = new Rectangle(0, 0, fixedFormatImage.Width, fixedFormatImage.Height);
            var bmpData = fixedFormatImage.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            // Get the address of the first line.
            var ptr = bmpData.Scan0;

            if (bmpData.Stride < 0)
            {
                Console.WriteLine("Upside-down");
                //http://stackoverflow.com/a/13273799/294804
                ptr += bmpData.Stride * (fixedFormatImage.Height - 1);
            }

            //http://stackoverflow.com/a/1917036/294804
            // Declare an array to hold the bytes of the bitmap. 
            var numBytes = bmpData.Stride * fixedFormatImage.Height;
            var rgbValues = new int[numBytes / 4];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, numBytes / 4);

            // Unlock the bits.
            fixedFormatImage.UnlockBits(bmpData);

            const int scaleFactor = 3;
            var scaledRbgValues = new int[numBytes / 4 * (scaleFactor * scaleFactor)];

            var xBrzConv = new xBrzConv();
            xBrzConv.ScaleImage(new ScaleSize(new Scaler3x()), rgbValues, scaledRbgValues, fixedFormatImage.Width, fixedFormatImage.Height, new ScalerCfg(), 0, int.MaxValue);

            var scaledImage = new Bitmap(fixedFormatImage.Width * scaleFactor, fixedFormatImage.Height * scaleFactor, pxf);
            var rect2 = new Rectangle(0, 0, scaledImage.Width, scaledImage.Height);
            var bmpData2 = scaledImage.LockBits(rect2, ImageLockMode.ReadWrite, pxf);

            // Get the address of the first line.
            var ptr2 = bmpData2.Scan0;

            //http://stackoverflow.com/a/1917036/294804
            // Declare an array to hold the bytes of the bitmap. 
            var numBytes2 = bmpData2.Stride * scaledImage.Height;

            // Copy the RGB values back to the bitmap
            Marshal.Copy(scaledRbgValues, 0, ptr2, numBytes2 / 4);

            // Unlock the bits.
            scaledImage.UnlockBits(bmpData);

            scaledImage.Save(fileName + "-" + scaleFactor + "xBRZ" + imageExtension, ImageFormat.Png);
        }
    }
}
