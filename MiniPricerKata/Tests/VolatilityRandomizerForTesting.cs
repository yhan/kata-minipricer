using MiniPricerKata.Impl2;

namespace MiniPricerKata.Tests
{
    public class VolatilityRandomizerForTesting : IRandomizeVolatility
    {
        private int _calledTimes;
        public int CalledTimes => _calledTimes;

        public double Randomrize(double volatility)
        {
            _calledTimes++;
            return volatility;
        }
    }
}