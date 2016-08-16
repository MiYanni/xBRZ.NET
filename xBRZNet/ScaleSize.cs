using System;

namespace xBRZNet
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

        public ScaleSize(IScaler scaler)
        {
            Scaler = scaler;
            Size = scaler.Scale;
        }

        // MJY: Changed return type to IScaler since ScaleSize is not an IScaler anymore.
        public static IScaler Cast(int ordinal)
        {
            var ord1 = Math.Max(ordinal, 0);
            var ord2 = Math.Min(ord1, Values.Length - 1);
            return Values[ord2];
        }

        public IScaler Scaler { get; }
        public int Size { get; }
    }
}
