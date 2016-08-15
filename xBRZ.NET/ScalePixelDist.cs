namespace xBRZ.NET
{
    internal class ScalePixelDist : ColorBase, IColorDist
    {
        public ScalePixelDist(ScalerCfg cfg) : base(cfg) { }

        public double _(int col1, int col2)
        {
            return ColorDist(col1, col2, Cfg.LuminanceWeight);
        }
    }
}
