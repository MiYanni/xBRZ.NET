namespace xBRZ.NET
{
    internal class ScalePixelEq : ColorBase, IColorEq
    {
        public ScalePixelEq(ScalerCfg cfg) : base(cfg) { }

        public bool _(int col1, int col2)
        {
            var eqColorThres = Common.Square(Cfg.EqualColorTolerance);
            return ColorDist(col1, col2, Cfg.LuminanceWeight) < eqColorThres;
        }
    }
}
