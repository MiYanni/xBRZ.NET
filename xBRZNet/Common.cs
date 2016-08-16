namespace xBRZNet
{
    internal static class Common
    {
        public const int MaxRotations = 4; // Number of 90 degree rotations
        public const int MaxPositions = 9;
        public const int MaxScale = 5; // Highest possible scale
        public const int MaxScaleSquared = MaxScale * MaxScale;

        //calculate input matrix coordinates after rotation at program startup
        public static readonly IntPair[] MatrixRotation;

        static Common()
        {
            MatrixRotation = new IntPair[(MaxScale - 1) * MaxScaleSquared * MaxRotations];
            for (var n = 2; n < MaxScale + 1; n++)
            {
                for (var r = 0; r < MaxRotations; r++)
                {
                    var nr = (n - 2) * (MaxRotations * MaxScaleSquared) + r * MaxScaleSquared;
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

        private static IntPair BuildMatrixRotation(int rotDeg, int i, int j, int n)
        {
            int iOld, jOld;

            if (rotDeg == 0)
            {
                iOld = i;
                jOld = j;

            }
            else
            {
                //old coordinates before rotation!
                var old = BuildMatrixRotation(rotDeg - 1, i, j, n);
                iOld = n - 1 - old.J;
                jOld = old.I;
            }

            return new IntPair(iOld, jOld);
        }

        public static double Square(this double value)
        {
            return value * value;
        }
    }
}
