namespace xBRZ.NET
{
    internal class ColorEq : ColorDist
    {
        public ColorEq(ScalerCfg cfg) : base(cfg) { }

        public bool IsColorEqual(int color1, int color2)
        {
            var eqColorThres = Common.Square(Cfg.EqualColorTolerance);
            return DistYCbCr(color1, color2) < eqColorThres;
        }
    }
}
