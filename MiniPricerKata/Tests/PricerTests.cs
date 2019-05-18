using System;
using System.Linq;
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
        protected internal DateTime D20190502 = new DateTime(2019, 5, 2);
        protected internal DateTime D20190503 = new DateTime(2019, 5, 3);
        protected internal DateTime D20190504 = new DateTime(2019, 5, 4);
        protected internal DateTime D20190505 = new DateTime(2019, 5, 5);
        protected internal DateTime D20190506 = new DateTime(2019, 5, 6);

        [Test]
        public void Should_return_current_price_When_requested_date_is_current_date()
        {
            var date = DateHelper.Make(2019, 4, 30);

            var pricer = new MiniPricer2(new Price(date, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

            var priceOf20190508 = pricer.GetPriceOf(date);

            Check.That(priceOf20190508).IsEqualTo(new Price(date, 100));
        }


        [Test]
        public void Should_provide_the_price_for_a_specific_day()
        {
            var today = DateHelper.Make(2019, 5, 1);
            var tomorrow = DateHelper.Make(2019, 5, 2);
            var pricer = new MiniPricer2(new Price(today, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

            var priceOf20190508 = pricer.GetPriceOf(tomorrow);

            Check.That(priceOf20190508).IsEqualTo(new Price(tomorrow, 120));
        }


        [Test]
        public void Should_not_apply_volatility_on_weekend()
        {
            var initialDate = DateHelper.Make(2019, 5, 2);

            var pricer = new MiniPricer2(new Price(initialDate, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

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
            var pricer = new MiniPricer2(new Price(initialDate, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

            var jf1 = pricer.GetPriceOf(new DateTime(2019, 5, 1));
            Check.That(jf1.Value).IsEqualTo(100);

            var jf2 = pricer.GetPriceOf(new DateTime(2019, 5, 5));
            Check.That(jf2.Value).IsEqualTo(100 * Math.Pow(1.2, 2)); // 2 3 4(saturday)

            var jf3 = pricer.GetPriceOf(new DateTime(2019, 5, 8));
            Check.That(jf3.Value).IsEqualTo(100 * Math.Pow(1.2, 4));
        }


        [TestCase(5, 0)] //monday
        [TestCase(6, 1)] //tuesday
        [TestCase(7, 2)] //wednesday
        [TestCase(8, 2)] // thursday
        [TestCase(9, 3)] // friday
        [TestCase(10, 4)] // saturday
        [TestCase(11, 4)] // sunday
        public void Should_apply_one_time_per_day_randomrize_volatility_When_the_date_is_neither_weekend_nor_jour_ferie(int day, int randomrizeCalledTimes)
        {
            var mondayFerie = new DateTime(2019, 5, 5);

            var date = new DateTime(2019, 5, day);
            var volatilityRandomizer = new VolatilityRandomizerForTesting();
            var pricer = new MiniPricer2(new Price(mondayFerie, InitialPrice), Volatility, new JoursFeriesProvider(), volatilityRandomizer);

            pricer.GetPriceOf(date);

            Check.That(volatilityRandomizer.CalledTimes).IsEqualTo(randomrizeCalledTimes);
        }

        
        [Test]
        public void Should_randomize_volatility_to_absoluteValue_or_negativeValue_or_zero()
        {
       
            var date = new DateTime(2019, 5, 1);
            var volatilityRandomizer = new VolatilityRandomizerTestDecorator();
            var pricer = new MiniPricer2(new Price(date, InitialPrice), Volatility, new JoursFeriesProvider(), volatilityRandomizer);

            pricer.GetPriceOf(date.AddDays(1000));

            var volatilities = volatilityRandomizer.Volatilities;
            Check.That(volatilities.All(v => Math.Abs(Math.Abs(v) - InitialPrice) < 0.000000001));
            Assert.That(volatilities.Any(v=> v < 0), Is.True, "Should have negative volatility");
            Assert.That(volatilities.Any(v=> v > 0), Is.True, "Should have posivite volatility");
            Assert.That(volatilities.Any(v=> v == 0), Is.True, "Should have null volatility");
        }
    }
}