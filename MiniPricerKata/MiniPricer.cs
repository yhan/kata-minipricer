using System;

namespace MiniPricerKata
{
    public class MiniPricer
    {
        private readonly Price _initialPrice;
        private readonly IProvideJoursFeries _joursFeriesProvider;
        private readonly IRandomizeVolatility _priceMoveTrendProvider;
        private readonly Volatility _volatility;
        private Basket _basket;

        public MiniPricer(Price initialPrice, Volatility volatility, IProvideJoursFeries joursFeriesProvider, 
            IRandomizeVolatility priceMoveTrendProvider, Basket basket)
        {
            _initialPrice = initialPrice;
            _volatility = volatility;
            _joursFeriesProvider = joursFeriesProvider;
            _priceMoveTrendProvider = priceMoveTrendProvider;
            _basket = basket;
        }

        public MiniPricer(Price initialPrice, Volatility volatility, IProvideJoursFeries joursFeriesProvider,
            IRandomizeVolatility priceMoveTrendProvider) : this(initialPrice, volatility, joursFeriesProvider, priceMoveTrendProvider,  new EmptyBasket())
        {

        }
        
        public Price GetPriceOf(Instrument instrument, DateTime date)
        {
            if (_basket.IsEmpty())
            {
                return InnerPriceOf(date);
            }

            throw new ArgumentException("Instrument is in a basket");
        }

        private Price InnerPriceOf(DateTime date)
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

                volatility = _priceMoveTrendProvider.Randomrize(volatility);

                price = price * (1 + volatility.Value / 100);
            }

            return new Price(date, price);
        }

        public BasketPriceComposition GetPriceOf(DateTime date, Basket basket)
        {
            var pivotPrice = InnerPriceOf(date);

            return new BasketPriceComposition(_basket, pivotPrice);
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