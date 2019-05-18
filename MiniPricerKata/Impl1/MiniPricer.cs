using System.Collections.Generic;
using System.Linq;

namespace MiniPricerKata.Impl1
{
    public class MiniPricer
    {
        private readonly Price _knownPrice;

        public MiniPricer(Price knownPrice)
        {
            _knownPrice = knownPrice;
        }

        public IEnumerable<Price> Extrapolate(ComplexVolatility[] volatilitySeries)
        {
            var current = _knownPrice;

            return volatilitySeries.Select((volatility, offset) =>
            {
                var date = _knownPrice.Date.AddDays(offset);

                var price = new Price(date, current.Value * (1 + volatility.Value / 100));
                current = price;

                return price;
            });
        }
    }
}