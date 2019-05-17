using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using NUnit.Framework;

namespace MiniPricerKata
{
    public class MiniPricerShould
    {
        //[Test]
        //public void Can_do_simple_extrapolation_Given_two_days_volatility_series()
        //{
        //    var volatility4m30 = new Volatility(0);//4/30
        //    var volatility5m1 = new Volatility(1);//1
        //    var volatility5m2 = new Volatility(2);//2
        //    var volatility5m3 = new Volatility(3);//1
        //    var volatility5m4 = new Volatility(42);//1
        //    var volatility5m5 = new Volatility(142);//1
        //    var volatility5m8 = new Volatility(999);//1
        //    var date = new DateTime(2019, 4, 30);//1 4 5 8
            
        //    var miniPricer = new MiniPricer(new Price(date, 42 ));
        //    var prices = miniPricer.Extrapolate(volatilitySeries: new [] {volatility4m30, volatility5m1, volatility5m2,volatility5m3, volatility5m4, volatility5m5, volatility5m8  } );

        //    Check.That(prices).ContainsExactly(
        //        new Price[]
        //        {
        //            new Price(date, 42), 
        //            new Price(date.AddDays(1), 42),
        //            new Price(date.AddDays(2), 42 * (1 + volatility5m2.Value / 100))
        //        } );
        //}

        [Test]
        public void Can_do_simple_extrapolation_Given_two_days_volatility_series()
        {
            var volatilityD0 = new Volatility(0);
            var volatilityD1 = new Volatility(1);
            var volatilityD2 = new Volatility(2);
            var date = new DateTime(1900, 1, 1);
            
            var miniPricer = new MiniPricer(new Price(date, 42 ));
            var prices = miniPricer.Extrapolate(volatilitySeries: new [] {volatilityD0, volatilityD1, volatilityD2} ).ToArray();

            var d1Price = 42 * (1 + volatilityD1.Value / 100);
            var d2Price = d1Price * (1 + volatilityD2.Value / 100);
            Check.That(prices).ContainsExactly(
                new Price[]
                {
                    new Price(date, 42), 
                    new Price(date.AddDays(1), d1Price),
                    new Price(date.AddDays(2), d2Price)
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
            var current = _knownPrice;

            return volatilitySeries.Select((volatility, offset) =>
            {
                
                var price = new Price(_knownPrice.Date.AddDays(offset), current.Value * (1 + volatility.Value / 100));
                current = price;

                return price;
            });
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
