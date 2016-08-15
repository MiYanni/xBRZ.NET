namespace xBRZ.NET
{
    internal class Rot
    {
        // Cache the 4 rotations of the 9 positions, a to i.
        public static readonly int[] _ = new int[9 * 4];

        static Rot()
        {
            const int a = 0, b = 1, c = 2,
                    d = 3, e = 4, f = 5,
                    g = 6, h = 7, i = 8;

            int[] deg0 = 
            {
                a,b,c,
                d,e,f,
                g,h,i
            };

            int[] deg90 = 
            {
                g,d,a,
                h,e,b,
                i,f,c
            };

            int[] deg180 = 
            {
                i,h,g,
                f,e,d,
                c,b,a
            };

            int[] deg270 = 
            {
                c,f,i,
                b,e,h,
                a,d,g
            };

            int[][] rotation = {deg0, deg90, deg180, deg270};

            for (var rotDeg = 0; rotDeg < 4; rotDeg++)
            {
                for (var x = 0; x < 9; x++)
                {
                    _[(x << 2) + rotDeg] = rotation[rotDeg][x];
                }
            }
        }
    }
}
