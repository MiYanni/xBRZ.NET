namespace xBRZ.NET
{
    internal class ColorBase
    {
        protected readonly ScalerCfg Cfg;

        public ColorBase(ScalerCfg cfg)
        {
            Cfg = cfg;
        }

        protected static double ColorDist(int pix1, int pix2, double luminanceWeight)
        {
            return pix1 == pix2 ? 0 : DistYCbCr(pix1, pix2, luminanceWeight);
        }

        private static double DistYCbCr(int pix1, int pix2, double lumaWeight)
        {
            //http://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
            //YCbCr conversion is a matrix multiplication => take advantage of linearity by subtracting first!
            var r_diff = ((pix1 & Common.RedMask) - (pix2 & Common.RedMask)) >> 16; //we may delay division by 255 to after matrix multiplication
            var g_diff = ((pix1 & Common.GreenMask) - (pix2 & Common.GreenMask)) >> 8; //
            var b_diff = (pix1 & Common.BlueMask) - (pix2 & Common.BlueMask); //subtraction for int is noticeable faster than for double

            var k_b = 0.0722; //ITU-R BT.709 conversion
            var k_r = 0.2126; //
            var k_g = 1 - k_b - k_r;

            var scale_b = 0.5 / (1 - k_b);
            var scale_r = 0.5 / (1 - k_r);

            var y = k_r * r_diff + k_g * g_diff + k_b * b_diff; //[!], analog YCbCr!
            var c_b = scale_b * (b_diff - y);
            var c_r = scale_r * (r_diff - y);

            // Skip division by 255.
            // Also skip square root here by pre-squaring the
            // config option equalColorTolerance.
            //return Math.sqrt(square(lumaWeight * y) + square(c_b) + square(c_r));
            return Common.Square(lumaWeight * y) + Common.Square(c_b) + Common.Square(c_r);
        }
    }
}
