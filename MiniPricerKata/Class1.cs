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
        public void Can_do_simple_extrapolation_Given_two_days_volatility_series()
        {
            var volatilityD0 = new Volatility(0);
            var volatilityD1 = new Volatility(1);
            var volatilityD2 = new Volatility(2);
            var date = new DateTime(1900, 1, 1);
            
            var miniPricer = new MiniPricer(new Price(date, 42 ));
            var prices = miniPricer.Extrapolate(volatilitySeries: new [] {volatilityD0, volatilityD1, volatilityD2} );

            Check.That(prices).ContainsExactly(
                new Price[]
                {
                    new Price(date, 42), 
                    new Price(date.AddDays(1), 42 * (1 + volatilityD1.Value / 100)),
                    new Price(date.AddDays(2), 42 * (1 + volatilityD2.Value / 100))
                } );
        }
    }

    public class MiniPricer
    {
        private readonly Price _knownPrice;

        public MiniPricer(Price knownPrice)
        {
            _knownPrice = knownPrice;
        }

        public IEnumerable<Price> Extrapolate(Volatility[] volatilitySeries)
        {
            return volatilitySeries.Select((volatility, offset) => new Price(_knownPrice.Date.AddDays(offset), _knownPrice.Value * (1 + volatility.Value / 100)));
        }
    }

    public struct Volatility
    {
        public double Value { get; }

        public Volatility(double value)
        {
            Value = value;
        }
    }

    public struct Price
    {
        public DateTime Date { get; }
        public double Value { get; }

        public Price(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }
    }
}
