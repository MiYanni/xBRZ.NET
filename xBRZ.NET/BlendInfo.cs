namespace xBRZ.NET
{
    internal class BlendInfo
    {
        public static char GetTopL(char b) { return (char)((b) & 0x3); }
        public static char GetTopR(char b) { return (char)((b >> 2) & 0x3); }
        public static char GetBottomR(char b) { return (char)((b >> 4) & 0x3); }
        public static char GetBottomL(char b) { return (char)((b >> 6) & 0x3); }

        public static char SetTopL(char b, char bt) { return (char)(b | bt); }
        public static char SetTopR(char b, char bt) { return (char)(b | (bt << 2)); }
        public static char SetBottomR(char b, char bt) { return (char)(b | (bt << 4)); }
        public static char SetBottomL(char b, char bt) { return (char)(b | (bt << 6)); }

        public static char Rotate(char b, RotationDegree rotDeg)
        {
            //assert rotDeg >= 0 && rotDeg < 4 : "RotationDegree enum does not have type: " + rotDeg;

            var l = (int)rotDeg << 1;
            var r = 8 - l;

            return (char)(b << l | b >> r);
        }
    }
}
