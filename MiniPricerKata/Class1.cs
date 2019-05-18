using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using NFluent;
using NUnit.Framework;

namespace MiniPricerKata
{
    public class MiniPricerShould
    {

        private static DateTime Make(int year, int month, int day)
        {
            return new DateTime(year, month, day);
        }

        [Test]
        public void Can_do_simple_extrapolation_Given_two_days_volatility_series()
        {
            var volatility4M30 = new Volatility(0, Make(2019, 4, 30));//4/30
            var volatility5M1 = new Volatility(1,  Make(2019,5,1));//5/1
            var volatility5M2 = new Volatility(2  , Make(2019,5,2) );
            var volatility5M3 = new Volatility(3  , Make(2019,5,3) );
            var volatility5M4 = new Volatility(42 , Make(2019,5,4)  );//5/4
            var volatility5M5 = new Volatility(142, Make(2019,5,5)   );//5/5
            var volatility5M6 = new Volatility(4  , Make(2019,5,6)  );//5/6
            var volatility5M7 = new Volatility(5  , Make(2019,5,7)  );//5/7
            var volatility5M8 = new Volatility(999, Make(2019,5,8)    );//5/8
            var date = new DateTime(2019, 4, 30);//1 4 5 8

            var miniPricer = new MiniPricer(new Price(date, 42));
            var prices = miniPricer.Extrapolate(volatilitySeries: new[] { volatility4M30,
                volatility5M1, volatility5M2, volatility5M3,
                volatility5M4, volatility5M5, 
                volatility5M6, volatility5M7,
                volatility5M8
            }).ToArray();



            Check.That(prices).ContainsExactly(
                new Price[]
                {
                    new Price(date, 42),
                    new Price(date.AddDays(1), 42),
                    new Price(date.AddDays(2), 42 * (1 + volatility5M2.Value / 100)),
                    new Price(date.AddDays(3), 42 * (1 + volatility5M2.Value / 100)* (1 + volatility5M3.Value / 100)),
                    new Price(date.AddDays(4), 42 * (1 + volatility5M2.Value / 100)* (1 + volatility5M3.Value / 100)),
                    new Price(date.AddDays(5), 42 * (1 + volatility5M2.Value / 100)* (1 + volatility5M3.Value / 100)),
                    new Price(date.AddDays(6), 42 * (1 + volatility5M2.Value / 100)* (1 + volatility5M3.Value / 100)* (1 + volatility5M6.Value / 100)),
                    new Price(date.AddDays(7), 42 * (1 + volatility5M2.Value / 100)* (1 + volatility5M3.Value / 100)* (1 + volatility5M6.Value / 100)* (1 + volatility5M7.Value / 100)),
                    new Price(date.AddDays(8), 42 * (1 + volatility5M2.Value / 100)* (1 + volatility5M3.Value / 100)* (1 + volatility5M6.Value / 100)* (1 + volatility5M7.Value / 100))
                });
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
                var date = _knownPrice.Date.AddDays(offset);
                
                var price = new Price(date, current.Value * (1 + volatility.Value / 100));
                current = price;

                return price;
            });
        }
    }


    public struct Volatility
    {
        private static readonly HashSet<DateTime> Feries = new HashSet<DateTime>{new DateTime(2019, 5, 1), new DateTime(2019, 5, 5), new DateTime(2019, 5, 8)};

        public double Value { get; }
        public DateTime Date { get; }

        public Volatility(double value, DateTime date)
        {
            Date = date;
            
         
            Value = Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday ||
                   Feries.Contains(date)
                ? 0
                : value;
                
        }
        

    }
    [DebuggerDisplay("{Date:yyyy-MM-dd)} {Value}")]
    public struct Price
    {
        public DateTime Date { get; }
        public double Value { get; }

        public Price(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }

        public override string ToString()
        {
            return $"{this.Date:yyyy-MM-dd} {this.Value}";
        }
    }
}
