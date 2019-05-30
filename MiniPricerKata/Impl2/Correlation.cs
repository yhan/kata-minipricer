using System;

namespace MiniPricerKata.Impl2
{
    public class Correlation
    {
        public virtual double Value { get; }

        public Correlation()
        {
            
        }

        public Correlation(double value)
        {
            Value = value;
        }
    }

    public class NullCorrection :Correlation
    {
        public NullCorrection() : base()
        {
        }

        public override double Value => throw new ArgumentException($"{typeof(NullCorrection)} has no value.");
    }
}