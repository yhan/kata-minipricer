using System;
using System.Collections.Generic;

namespace MiniPricerKata.Impl1
{
    public struct ComplexVolatility
    {
        private static readonly HashSet<DateTime> Feries = new HashSet<DateTime>
            {new DateTime(2019, 5, 1), new DateTime(2019, 5, 5), new DateTime(2019, 5, 8)};

        public double Value { get; }
        public DateTime Date { get; }

        public ComplexVolatility(double value, DateTime date)
        {
            Date = date;

            Value = Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday ||
                    Feries.Contains(date)
                ? 0
                : value;
        }
    }
}