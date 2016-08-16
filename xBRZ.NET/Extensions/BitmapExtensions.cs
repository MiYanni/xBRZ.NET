using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xBRZ.NET.Extensions
{
    public static class BitmapExtensions
    {
        //http://stackoverflow.com/a/2016509/294804
        public static Bitmap ChangeFormat(this Bitmap image, PixelFormat format)
        {
            var newFormatImage = new Bitmap(image.Width, image.Height, format);
            using (var gr = Graphics.FromImage(newFormatImage))
            {
                gr.DrawImage(image, new Rectangle(0, 0, newFormatImage.Width, newFormatImage.Height));
            }
            return newFormatImage;
        }
    }
}
