using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniPricerKata.Impl2
{
    public class MonteCarloVolatilityRandomizer : IRandomizeVolatility
    {
        private readonly int _largeNumber;
        private readonly Volatility _volatilitySeed;
        private readonly Func<double, double> _volatilityProducer;

        public MonteCarloVolatilityRandomizer(int largeNumber, Volatility volatilitySeed,  Func<double, double> volatilityProducer)
        {
            _largeNumber = largeNumber;
            _volatilitySeed = volatilitySeed;
            _volatilityProducer = volatilityProducer;
        }

        public double Randomrize(double volatility)
        {
            var buffer = new double[_largeNumber];
            Parallel.For(0, _largeNumber, x => { buffer[x] = _volatilityProducer(_volatilitySeed.Value); });

            return buffer.Sum() / _largeNumber;
        }
    }
}