using System;

namespace MiniPricerKata.Impl2
{
    public class MiniPricer2
    {
        private readonly Price _initialPrice;
        private readonly IProvideJoursFeries _joursFeriesProvider;
        private readonly Volatility _volatility;

        public MiniPricer2(Price initialPrice, Volatility volatility, IProvideJoursFeries joursFeriesProvider)
        {
            _initialPrice = initialPrice;
            _volatility = volatility;
            _joursFeriesProvider = joursFeriesProvider;
        }

        public Price GetPriceOf(DateTime date)
        {
            var numberOfDays = date.Subtract(_initialPrice.Date).Days;
            var volatility = _volatility.Value;

            var price = _initialPrice.Value;

            for (var offset = 1; offset <= numberOfDays; offset++)
            {
                var currentDate = _initialPrice.Date.AddDays(offset);

                volatility = _volatility.Value;
                if (IsWeekend(currentDate) || IsJourFerie(currentDate))
                {
                    volatility = 0;
                }

                price = price * (1 + volatility / 100);
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