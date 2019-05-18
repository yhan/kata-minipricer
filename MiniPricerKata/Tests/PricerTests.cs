using System;
using MiniPricerKata.Impl1;
using MiniPricerKata.Impl2;
using NFluent;
using NUnit.Framework;

namespace MiniPricerKata.Tests
{
    public class PricerTests
    {
        private const int InitialPrice = 100;
        private static readonly Volatility Volatility = new Volatility(20);

        [Test]
        public void Should_return_current_price_When_requested_date_is_current_date()
        {
            var date = DateHelper.Make(2019, 4, 30);

            var pricer = new MiniPricer2(new Price(date, InitialPrice), Volatility, new JoursFeriesProvider());

            var priceOf20190508 = pricer.GetPriceOf(date);

            Check.That(priceOf20190508).IsEqualTo(new Price(date, 100));
        }


        [Test]
        public void Should_provide_the_price_for_a_specific_day()
        {
            var today = DateHelper.Make(2019, 5, 1);
            var tomorrow = DateHelper.Make(2019, 5, 2);
            var pricer = new MiniPricer2(new Price(today, InitialPrice), Volatility, new JoursFeriesProvider());

            var priceOf20190508 = pricer.GetPriceOf(tomorrow);

            Check.That(priceOf20190508).IsEqualTo(new Price(tomorrow, 120));
        }


        [Test]
        public void Should_not_apply_volatility_on_weekend()
        {
            var initialDate = DateHelper.Make(2019, 5, 2);

            var pricer = new MiniPricer2(new Price(initialDate, InitialPrice), Volatility, new JoursFeriesProvider());

            var friday = DateHelper.Make(2019, 5, 3);

            var saturday = friday.AddDays(1);
            var sunday = saturday.AddDays(1);

            var priceOfSaturday = pricer.GetPriceOf(saturday);
            Check.That(priceOfSaturday.Value).IsEqualTo(120);

            var priceOfSunday = pricer.GetPriceOf(sunday);
            Check.That(priceOfSunday.Value).IsEqualTo(120);
        }


        [Test]
        public void Should_not_apply_volatility_on_jours_feries()
        {
            var initialDate = DateHelper.Make(2019, 4, 30);
            var pricer = new MiniPricer2(new Price(initialDate, InitialPrice), Volatility, new JoursFeriesProvider());

            var jf1 = pricer.GetPriceOf(new DateTime(2019, 5, 1));
            Check.That(jf1.Value).IsEqualTo(100);

            var jf2 = pricer.GetPriceOf(new DateTime(2019, 5, 5));
            Check.That(jf2.Value).IsEqualTo(100 * Math.Pow(1.2, 2)); // 2 3 4(saturday)

            var jf3 = pricer.GetPriceOf(new DateTime(2019, 5, 8));
            Check.That(jf3.Value).IsEqualTo(100 * Math.Pow(1.2, 4));
        }
    }
}