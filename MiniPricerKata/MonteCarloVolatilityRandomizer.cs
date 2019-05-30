using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniPricerKata
{
    public class MonteCarloVolatilityRandomizer : IRandomizeVolatility
    {
        private readonly int _largeNumber;
        private readonly Volatility _volatilitySeed;
        private readonly Func<Volatility, Volatility> _volatilityProducer;

        public MonteCarloVolatilityRandomizer(int largeNumber, Volatility volatilitySeed,  Func<Volatility, Volatility> volatilityProducer)
        {
            _largeNumber = largeNumber;
            _volatilitySeed = volatilitySeed;
            _volatilityProducer = volatilityProducer;
        }

        public Volatility Randomrize(Volatility volatility)
        {
            var buffer = new double[_largeNumber];
            Parallel.For(0, _largeNumber, x => { buffer[x] = _volatilityProducer(_volatilitySeed).Value; });

            return new Volatility(buffer.Sum() / _largeNumber);
        }
    }
}