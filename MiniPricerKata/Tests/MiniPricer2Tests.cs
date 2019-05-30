using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MiniPricerKata.Impl2;
using NFluent;
using NUnit.Framework;

namespace MiniPricerKata.Tests
{
    public class MiniPricer2Tests
    {
        private const int InitialPrice = 100;
        private static readonly Volatility Volatility = new Volatility(20);
        protected internal DateTime D20190430 = new DateTime(2019, 4, 30);
        protected internal DateTime D20190501 = new DateTime(2019, 5, 1);
        protected internal DateTime D20190502 = new DateTime(2019, 5, 2);
        protected internal DateTime D20190503 = new DateTime(2019, 5, 3);
        protected internal DateTime D20190504 = new DateTime(2019, 5, 4);
        protected internal DateTime D20190505 = new DateTime(2019, 5, 5);
        protected internal DateTime D20190506 = new DateTime(2019, 5, 6);
        protected internal DateTime D20190508 = new DateTime(2019, 5, 8);

        [Test]
        public void Should_return_current_price_When_requested_date_is_current_date()
        {
            var date = D20190430;

            var pricer = new MiniPricer2(new Price(date, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

            var priceOf20190508 = pricer.GetPriceOf(date);

            Check.That(priceOf20190508).IsEqualTo(new Price(date, InitialPrice));
        }


        [Test]
        public void Should_provide_the_price_for_a_specific_day()
        {
            var today = D20190501;
            var tomorrow = D20190502;
            var pricer = new MiniPricer2(new Price(today, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

            var priceOf20190508 = pricer.GetPriceOf(tomorrow);

            Check.That(priceOf20190508).IsEqualTo(new Price(tomorrow, InitialPrice + Volatility.Value));
        }


        [Test]
        public void Should_not_apply_volatility_on_weekend()
        {
            var initialDate = D20190502;

            var pricer = new MiniPricer2(new Price(initialDate, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

            var friday = D20190503;

            var saturday = friday.AddDays(1);
            var sunday = saturday.AddDays(1);

            var priceOfSaturday = pricer.GetPriceOf(saturday);
            Check.That(priceOfSaturday.Value).IsEqualTo(InitialPrice + Volatility.Value);

            var priceOfSunday = pricer.GetPriceOf(sunday);
            Check.That(priceOfSunday.Value).IsEqualTo(InitialPrice + Volatility.Value);
        }


        [Test]
        public void Should_not_apply_volatility_on_jours_feries()
        {
            var initialDate = D20190430;
            var pricer = new MiniPricer2(new Price(initialDate, InitialPrice), Volatility, new JoursFeriesProvider(), new VolatilityRandomizerForTesting());

            var jf1 = pricer.GetPriceOf(D20190501);
            Check.That(jf1.Value).IsEqualTo(InitialPrice);

            var jf2 = pricer.GetPriceOf(D20190505);
            Check.That(jf2.Value).IsEqualTo(InitialPrice * Math.Pow((InitialPrice + Volatility.Value) / InitialPrice, 2)); // 2 3 4(saturday)

            var jf3 = pricer.GetPriceOf(D20190508);
            Check.That(jf3.Value).IsEqualTo(InitialPrice * Math.Pow((InitialPrice + Volatility.Value) / InitialPrice, 4));
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
            var mondayFerie = D20190505;

            var date = new DateTime(2019, 5, day);
            var volatilityRandomizer = new VolatilityRandomizerForTesting();
            var pricer = new MiniPricer2(new Price(mondayFerie, InitialPrice), Volatility, new JoursFeriesProvider(), volatilityRandomizer);

            pricer.GetPriceOf(date);

            Check.That(volatilityRandomizer.CalledTimes).IsEqualTo(randomrizeCalledTimes);
        }


        [Test]
        public void Should_randomize_volatility_to_absoluteValue_or_negativeValue_or_zero()
        {

            var date = D20190501;
            var volatilityRandomizer = new VolatilityRandomizerTestDecorator();
            var pricer = new MiniPricer2(new Price(date, InitialPrice), Volatility, new JoursFeriesProvider(), volatilityRandomizer);

            pricer.GetPriceOf(date.AddDays(1000));

            var volatilities = volatilityRandomizer.Volatilities.ToArray();
            Check.That(volatilities.All(v => Math.Abs(Math.Abs(v) - InitialPrice) < 0.000000001));
            Assert.That(volatilities.Any(v => v < 0), Is.True, "Should have negative volatility");
            Assert.That(volatilities.Any(v => v > 0), Is.True, "Should have posivite volatility");
            Assert.That(volatilities.Any(v => Math.Abs(v) < 0.000001), Is.True, "Should have null volatility");
        }

        [Test]
        public void Can_apply_random_volatility_using_monte_carlo()
        {
            var date = D20190505;
            var nextDate = date.AddDays(1);

            var queue = new Queue<double>();
            queue.Enqueue(20);
            queue.Enqueue(4);
            queue.Enqueue(30);

            Func<Volatility, Volatility> volatilityProducer = d => new Volatility(queue.Dequeue());

            var volatilityRandomizer = new MonteCarloVolatilityRandomizer(3, Volatility, volatilityProducer);
            var pricer = new MiniPricer2(new Price(date, InitialPrice), Volatility, new JoursFeriesProvider(), volatilityRandomizer);

            var priceOfNextDate = pricer.GetPriceOf(nextDate);

            Check.That(priceOfNextDate).IsEqualTo(new Price(nextDate, 100 * (1 + (20d + 4 + 30) / 3 / 100)));
        }

        [Test]
        public void Should_get_correct_price_range_When_apply_monte_carlo()
        {
            var date = D20190505;
            var nextDate = date.AddDays(1);
            int largeNumber = 10000;

            int randomingCount = 0;
            Func<Volatility, Volatility> producer = d =>
            {
                var volatilityRandomizer = new VolatilityRandomizer();
                Interlocked.Increment(ref randomingCount);
                return volatilityRandomizer.Randomrize(Volatility);
            };

            var monteCarlo = new MonteCarloVolatilityRandomizer(largeNumber, Volatility, producer);

            var pricer = new MiniPricer2(new Price(date, InitialPrice), Volatility, new JoursFeriesProvider(), monteCarlo);

            var priceOfNextDay = pricer.GetPriceOf(nextDate);

            Check.That(randomingCount).IsEqualTo(largeNumber);
            Check.That(priceOfNextDay.Date).IsEqualTo(nextDate);
            Check.That(priceOfNextDay.Value).IsStrictlyLessThan(120);
            Check.That(priceOfNextDay.Value).IsStrictlyGreaterThan(80);
        }
    }

    //[TestFixture]
    //public class stuffShould
    //{
    //    [Test]
    //    public void fqdsfqs()
    //    {
    //        Check.That(3 ^ 2).IsEqualTo(9);
    //    }
    //}
}