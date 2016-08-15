namespace xBRZ.NET
{
    internal static class Common
    {
        public static readonly int RedMask = 0xff0000;
        public static readonly int GreenMask = 0x00ff00;
        public static readonly int BlueMask = 0x0000ff;

        public static readonly int MaxRots = 4; // Number of 90 degree rotations
        public static readonly int MaxScale = 5; // Highest possible scale
        public static readonly int MaxScaleSq = MaxScale * MaxScale;

        //calculate input matrix coordinates after rotation at program startup
        public static readonly IntPair[] MatrixRotation;

        static Common()
        {
            MatrixRotation = new IntPair[(MaxScale - 1) * MaxScaleSq * MaxRots];
            for (var n = 2; n < MaxScale + 1; n++)
            {
                for (var r = 0; r < MaxRots; r++)
                {
                    var nr = (n - 2) * (MaxRots * MaxScaleSq) + r * MaxScaleSq;
                    for (var i = 0; i < MaxScale; i++)
                    {
                        for (var j = 0; j < MaxScale; j++)
                        {
                            MatrixRotation[nr + i * MaxScale + j] = BuildMatrixRotation(r, i, j, n);
                        }
                    } 
                }
            }
        }

        private static IntPair BuildMatrixRotation(int rotDeg, int I, int J, int N)
        {
            int I_old, J_old;

            if (rotDeg == 0)
            {
                I_old = I;
                J_old = J;

            }
            else
            {
                //old coordinates before rotation!
                var old = BuildMatrixRotation(rotDeg - 1, I, J, N);
                I_old = N - 1 - old.J;
                J_old = old.I;
            }

            return new IntPair(I_old, J_old);
        }

        public static double Square(double value)
        {
            return value * value;
        }
    }
}
