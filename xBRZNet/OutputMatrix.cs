namespace xBRZNet
{
    //access matrix area, top-left at position "out" for image with given width
    public class OutputMatrix
    {
        private readonly IntPtr _out;
        private readonly int _outWidth;
        private readonly int _n;
        private int _outi;
        private int _nr;

        public OutputMatrix(int scale, int[] outPtr, int outWidth)
        {
            _n = (scale - 2) * (Common.MaxRotations * Common.MaxScaleSquared);
            _out = new IntPtr(outPtr);
            _outWidth = outWidth;
        }

        public void Move(int rotDeg, int outi)
        {
            _nr = _n + rotDeg * Common.MaxScaleSquared;
            _outi = outi;
        }

        public IntPtr Ref(int i, int j)
        {
            var rot = Common.MatrixRotation[_nr + i * Common.MaxScale + j];
            _out.Position(_outi + rot.J + rot.I * _outWidth);
            return _out;
        }
    }
}
