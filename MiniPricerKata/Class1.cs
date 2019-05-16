using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using NUnit.Framework;

namespace MiniPricerKata
{
    public class MiniPricerShould
    {
        [Test]
        public void Can_do_simple_extrapolation_Given_one_day_volatility()
        {
            double volatility = 1;
            var prices = MiniPricer.Extrapolate(initialPrice: 42, volatilitySeries: new double[] {0, volatility});
            Check.That(prices).ContainsExactly(42, 42 * (1 + volatility / 100));
        }

        
        [Test]
        public void Can_do_simple_extrapolation_Given_two_days_volatility_series()
        {
            double volatilityD1 = 1;
            double volatilityD2 = 2;
            var prices = MiniPricer.Extrapolate(initialPrice: 42, volatilitySeries: new double[] {0, volatilityD1, volatilityD2});
            Check.That(prices).ContainsExactly(42, 42 * (1 + volatilityD1 / 100), 42 * (1 + volatilityD2 / 100));
        }
    }

    public class MiniPricer
    {
        public static IEnumerable<double> Extrapolate(double initialPrice, double[] volatilitySeries)
        {
            return volatilitySeries.Select(x => initialPrice * (1 + x / 100));
        }
    }
}
