﻿using System.Collections.Generic;

namespace MiniPricerKata.Tests
{
    public class VolatilityRandomizerTestDecorator : IRandomizeVolatility
    {
        private VolatilityRandomizer _innerRandomrizer;

        public VolatilityRandomizerTestDecorator()
        {
            _innerRandomrizer = new VolatilityRandomizer();
        }

        public Volatility Randomrize(Volatility volatility)
        {
            var randomrizedVolatility = _innerRandomrizer.Randomrize(volatility);

            _volatilities.Add(randomrizedVolatility.Value);
            
            return volatility;
        }

        public IEnumerable<double> Volatilities => _volatilities;

        private List<double> _volatilities = new List<double>();
    }
}