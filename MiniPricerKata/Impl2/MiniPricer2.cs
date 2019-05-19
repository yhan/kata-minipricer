using System;

namespace MiniPricerKata.Impl2
{
    public class MiniPricer2
    {
        private readonly Price _initialPrice;
        private readonly IProvideJoursFeries _joursFeriesProvider;
        private readonly IRandomizeVolatility _priceMoveTrendProvider;
        private readonly Volatility _volatility;

        public MiniPricer2(Price initialPrice, Volatility volatility, IProvideJoursFeries joursFeriesProvider,
            IRandomizeVolatility priceMoveTrendProvider)
        {
            _initialPrice = initialPrice;
            _volatility = volatility;
            _joursFeriesProvider = joursFeriesProvider;
            _priceMoveTrendProvider = priceMoveTrendProvider;
        }

        public MiniPricer2(Price initialPrice, Volatility volatility, IProvideJoursFeries joursFeriesProvider)
            : this(initialPrice, volatility, joursFeriesProvider, new VolatilityRandomizer())
        {
            
        }

        public Price GetPriceOf(DateTime date)
        {
            var numberOfDays = date.Subtract(_initialPrice.Date).Days;

            var price = _initialPrice.Value;

            for (var offset = 1; offset <= numberOfDays; offset++)
            {
                var currentDate = _initialPrice.Date.AddDays(offset);

                var volatility = _volatility;
                if (IsWeekend(currentDate) || IsJourFerie(currentDate))
                {
                    continue;
                }

                volatility =  _priceMoveTrendProvider.Randomrize(volatility);

                price = price * (1 + volatility.Value / 100);
            }

            return new Price(date, price);
        }

        private bool IsJourFerie(DateTime date)
        {
            return _joursFeriesProvider.IsJourFerie(date);
        }


        private static bool IsWeekend(DateTime currentDate)
        {
            return currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}