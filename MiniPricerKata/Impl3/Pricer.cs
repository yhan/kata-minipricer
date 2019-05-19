using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MiniPricer_csharp
{
    public class MiniPricerTests
    {
        private Pricer _pricer;
        private DateTime TODAY = new DateTime(2015, 07, 20); // monday 20 july 2015
        private DateTime TOMOROW = new DateTime(2015, 07, 21); // tuesday 21 july 2015
        private const int CURRENT_PRICE = 100;
        private const double CURRENT_VOLATILITY = 20.00;


        [SetUp]
        public void Setup()
        {
            _pricer = new Pricer();
        }


        [Test]
        public void Should_Return_CurrentPrice_When_FutureDateIsActualDate()
        {
            var futureDate = TODAY;

            var result = _pricer.GetPriceFor(TODAY, futureDate, CURRENT_PRICE, CURRENT_VOLATILITY);

            Assert.AreEqual(CURRENT_PRICE, result);
        }

        [TestCase(1, 120.0)]
        [TestCase(2, 144.0)]
        [TestCase(3, 172.8)]
        [TestCase(4, 207.36)]
        public void Should_apply_one_time_by_days_the_volatility_on_price_(int numberOfDays, double expectedPrice)
        {
            var callCount = 0;
            Func<double, double> getVolatilityTestWapper = d =>
            {
                callCount++;
                return CURRENT_VOLATILITY;
            };

            _pricer = new Pricer(Pricer.IsWeekEndDayOffChecker, getVolatilityTestWapper);

            var futureDate = TODAY.AddDays(numberOfDays);

            var result = _pricer.GetPriceFor(TODAY, futureDate, CURRENT_PRICE, CURRENT_VOLATILITY);

            Assert.AreEqual(expectedPrice, result, 0.0000000001);
            Assert.AreEqual(numberOfDays, callCount);
        }

        [Test]
        public void Should_not_apply_volatility_on_price_durring_weekend_by_default()
        {
            var friday = new DateTime(2015, 07, 31);
            var saturday = friday.AddDays(1);
            var sunday = saturday.AddDays(1);

            var saturdayPrice = _pricer.GetPriceFor(friday, saturday, CURRENT_PRICE, CURRENT_VOLATILITY);
            Assert.AreEqual(CURRENT_PRICE, saturdayPrice, 0.0000000001, "price for saturday is not the same as friday");

            var sundayPrice = _pricer.GetPriceFor(friday, sunday, CURRENT_PRICE, CURRENT_VOLATILITY);
            Assert.AreEqual(CURRENT_PRICE, sundayPrice, 0.0000000001, "price for sunday is not the same as friday");
        }

        [Test]
        public void Should_not_apply_volatility_on_price_day_off()
        {

            var dayOff = new[] {
                TODAY.AddDays(1),
                TODAY.AddDays(3),
                TODAY.AddDays(4)
            };

            var callCount = 0;
            Func<double, double> getVolatilityTestWapper = d =>
            {
                callCount++;
                return CURRENT_VOLATILITY;
            };

            Func<DateTime, bool> isDayOffTestWrapper = date => dayOff.Contains(date);

            _pricer = new Pricer(isDayOffTestWrapper, getVolatilityTestWapper);

            var newPrice = _pricer.GetPriceFor(TODAY, dayOff.Last(), CURRENT_PRICE, CURRENT_VOLATILITY);
            Assert.AreEqual(120.0, newPrice, 0.0000000001);
            Assert.AreEqual(1, callCount);

        }

        [Test]
        public void Should_apply_PositiveNegativeOrNone_Volitility_each_day_randomly()
        {
            var volatilities = new List<double>();

            Func<double, double> getVolatilityTestWapper = currentVolatility =>
            {
                var nextVolatility = Pricer.GetNextRandomVolatility(currentVolatility);
                volatilities.Add(nextVolatility);
                return nextVolatility;
            };

            var pricer = new Pricer(Pricer.IsWeekEndDayOffChecker, getVolatilityTestWapper);

            pricer.GetPriceFor(TODAY, TODAY.AddDays(10000), CURRENT_PRICE, CURRENT_VOLATILITY);

            Assert.IsTrue(volatilities.Any(v => v < 0), "Some volatilities are positive");
            Assert.IsTrue(volatilities.Any(v => Math.Abs(v) < 0.00000000001), "Some volatilities are null");
            Assert.IsTrue(volatilities.Any(v => v > 0), "Some volatilities are negative");
        }

        [Test]
        public void Should_Return_many_time_volatility_calculation_When_use_MonteCarlo_as_Volatility_average()
        {
            var callCount = 0;
            Func<double, double> getVolatilityTestWapper = d =>
            {
                Interlocked.Increment(ref callCount);
                return Pricer.GetNextRandomVolatility(d);
            };

            Func<double, double> monteCarloFor1000Shot = Pricer.GetMonteCarloVolatilityFor(1000, getVolatilityTestWapper);

            var pricer = new Pricer(Pricer.IsWeekEndDayOffChecker, monteCarloFor1000Shot);
            var result = pricer.GetPriceFor(TODAY, TOMOROW, CURRENT_PRICE, CURRENT_VOLATILITY);

            Assert.AreEqual(1000, callCount);
            Assert.GreaterOrEqual(120.00, result); //120 > result
            Assert.LessOrEqual(80.00, result);      //80 < result
        }

        [Test]
        public void Should_Return_average_volatility_calculation_When_use_MonteCarlo_()
        {
            var volatilities = new Stack<double>();
            volatilities.Push(20d);
            volatilities.Push(4d);
            volatilities.Push(30d);

            Func<double, double> getVolatilityTestWapper = d =>
            {
                return volatilities.Pop();
            };

            Func<double, double> monteCarloTest = Pricer.GetMonteCarloVolatilityFor(volatilities.Count, getVolatilityTestWapper);

            var result = monteCarloTest(Double.NaN);

            Assert.AreEqual((20d + 4d + 30d) / 3, result, 0.0000000001);
        }

    }

    public enum VolatilityChangeTypes
    {
        Up = 0,
        Down = 1,
        None = 2
    }

    public class Pricer
    {
        private readonly Func<double, double> _getVolatility = GetNextRandomVolatility;
        private readonly Func<DateTime, bool> _isDayOff = IsWeekEndDayOffChecker;

        public Pricer()
        {
        }

        public Pricer(Func<DateTime, bool> isDayOff)
        {
            if (isDayOff != null)
            {
                _isDayOff = isDayOff;
            }
        }

        public Pricer(Func<DateTime, bool> isDayOff, Func<double, double> getVolatility)
        {
            if (getVolatility != null) { _getVolatility = getVolatility; }
            if (isDayOff != null) { _isDayOff = isDayOff; }
        }

        public double GetPriceFor(DateTime today, DateTime futureDate, double currentPrice, double averageVolatility)
        {
            var numberOfDays = futureDate.Subtract(today).Days;
            var currentDay = today;
            var newPrice = currentPrice;
            for (int i = 0; i < numberOfDays; i++)
            {
                currentDay = currentDay.AddDays(1);
                if (!_isDayOff(currentDay))
                {
                    newPrice *= (1 + _getVolatility(averageVolatility) / 100);
                }
            }
            return newPrice;
        }

        public static bool IsWeekEndDayOffChecker(DateTime currentDay)
        {
            return currentDay.DayOfWeek == DayOfWeek.Saturday || currentDay.DayOfWeek == DayOfWeek.Sunday;
        }


        static readonly Random _randomForVolatility = new Random();
        public static double GetNextRandomVolatility(double currentVolatility)
        {
            var nextVolatilityChange = (VolatilityChangeTypes)_randomForVolatility.Next(3);

            switch (nextVolatilityChange)
            {
                case VolatilityChangeTypes.Up:
                    return Math.Abs(currentVolatility);
                case VolatilityChangeTypes.Down:
                    return 0 - Math.Abs(currentVolatility);
                case VolatilityChangeTypes.None:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Func<double, double> GetMonteCarloVolatilityFor(int times, Func<double, double> getVolatility)
        {
            return volatility =>
            {
                var volatilities = new double[times];
                var result = Parallel.For(0, times, i =>
                {
                    volatilities[i] = getVolatility(volatility);
                });



                return volatilities.Average();
            };
        }
    }
}