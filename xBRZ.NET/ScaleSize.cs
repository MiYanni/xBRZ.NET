using System;

namespace xBRZ.NET
{
    public class ScaleSize
    {
        private static readonly IScaler[] Values =
        {
            new Scaler2x(),
            new Scaler3x(),
            new Scaler4x(),
            new Scaler5x()
        };
        //Times2(Scaler2x),
        //Times3(Scaler3x),
        //Times4(Scaler4x),
        //Times5(Scaler5x)

        public ScaleSize(IScaler scaler)
        {
            Scaler = scaler;
            Size = scaler.Scale();
        }

        // MJY: Changed return type to IScaler since ScaleSize is not an IScaler anymore.
        public static IScaler Cast(int ordinal)
        {
            var ord1 = Math.Max(ordinal, 0);
            var ord2 = Math.Min(ord1, Values.Length - 1);
            return Values[ord2];
        }

        public IScaler Scaler;
        public int Size;
    }
}
