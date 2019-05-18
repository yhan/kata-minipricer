using System;
using System.Linq;
using NFluent;
using NUnit.Framework;

namespace MiniPricerKata.Impl1
{
    public class MiniPricerShould
    {
        [Test]
        public void Can_do_simple_extrapolation()
        {
            var volatility4M30 = new ComplexVolatility(0, DateHelper.Make(2019, 4, 30)); //4/30
            var volatility5M1 = new ComplexVolatility(1, DateHelper.Make(2019, 5, 1)); //5/1
            var volatility5M2 = new ComplexVolatility(2, DateHelper.Make(2019, 5, 2));
            var volatility5M3 = new ComplexVolatility(3, DateHelper.Make(2019, 5, 3));
            var volatility5M4 = new ComplexVolatility(42, DateHelper.Make(2019, 5, 4)); //5/4
            var volatility5M5 = new ComplexVolatility(142, DateHelper.Make(2019, 5, 5)); //5/5
            var volatility5M6 = new ComplexVolatility(4, DateHelper.Make(2019, 5, 6)); //5/6
            var volatility5M7 = new ComplexVolatility(5, DateHelper.Make(2019, 5, 7)); //5/7
            var volatility5M8 = new ComplexVolatility(999, DateHelper.Make(2019, 5, 8)); //5/8
            var date = new DateTime(2019, 4, 30); //1 4 5 8

            var miniPricer = new MiniPricer(new Price(date, 42));
            var prices = miniPricer.Extrapolate(new[]
            {
                volatility4M30,
                volatility5M1, volatility5M2, volatility5M3,
                volatility5M4, volatility5M5,
                volatility5M6, volatility5M7,
                volatility5M8
            }).ToArray();


            Check.That(prices).ContainsExactly(new Price(date, 42), new Price(date.AddDays(1), 42),
                new Price(date.AddDays(2), 42 * (1 + volatility5M2.Value / 100)),
                new Price(date.AddDays(3), 42 * (1 + volatility5M2.Value / 100) * (1 + volatility5M3.Value / 100)),
                new Price(date.AddDays(4), 42 * (1 + volatility5M2.Value / 100) * (1 + volatility5M3.Value / 100)),
                new Price(date.AddDays(5), 42 * (1 + volatility5M2.Value / 100) * (1 + volatility5M3.Value / 100)),
                new Price(date.AddDays(6),
                    42 * (1 + volatility5M2.Value / 100) * (1 + volatility5M3.Value / 100) *
                    (1 + volatility5M6.Value / 100)),
                new Price(date.AddDays(7),
                    42 * (1 + volatility5M2.Value / 100) * (1 + volatility5M3.Value / 100) *
                    (1 + volatility5M6.Value / 100) * (1 + volatility5M7.Value / 100)),
                new Price(date.AddDays(8),
                    42 * (1 + volatility5M2.Value / 100) * (1 + volatility5M3.Value / 100) *
                    (1 + volatility5M6.Value / 100) * (1 + volatility5M7.Value / 100)));
        }
    }
}