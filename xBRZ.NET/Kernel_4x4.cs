namespace xBRZ.NET
{
    /*
        input kernel area naming convention:
        -----------------
        | A | B | C | D |
        ----|---|---|---|
        | E | F | G | H | //evalute the four corners between F, G, J, K
        ----|---|---|---| //input pixel is at position F
        | I | J | K | L |
        ----|---|---|---|
        | M | N | O | P |
        -----------------
    */
    internal class Kernel_4x4
    {
        public int a, b, c, d;
        public int e, f, g, h;
        public int i, j, k, l;
        public int m, n, o, p;
    }
}
