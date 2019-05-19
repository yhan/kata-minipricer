using System;

namespace MiniPricerKata.Impl2
{
    class VolatilityRandomizer : IRandomizeVolatility
    {
        private static readonly Random Random = new Random();

        public Volatility Randomrize(Volatility volatility)
        {
            var next = Random.Next(3);
            int sign;
            switch ((PriceMoveTrend)next)
            {
                case PriceMoveTrend.Down:
                    sign = -1;
                    break;
                case PriceMoveTrend.Flat:
                    sign = 0;
                    break;
                case PriceMoveTrend.Up:
                    sign = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Volatility(volatility.Value * sign);
        }
    }
}