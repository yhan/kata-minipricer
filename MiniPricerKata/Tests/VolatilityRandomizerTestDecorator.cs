using System.Collections.Generic;
using MiniPricerKata.Impl2;

namespace MiniPricerKata.Tests
{
    public class VolatilityRandomizerTestDecorator : IRandomizeVolatility
    {
        private VolatilityRandomizer _innerRandomrizer;

        public VolatilityRandomizerTestDecorator()
        {
            _innerRandomrizer = new VolatilityRandomizer();
        }

        public double Randomrize(double volatility)
        {
            var randomrizedVolatility = _innerRandomrizer.Randomrize(volatility);

            _volatilities.Add(randomrizedVolatility);
            
            return volatility;
        }

        public IEnumerable<double> Volatilities => _volatilities;

        private List<double> _volatilities = new List<double>();
    }
}