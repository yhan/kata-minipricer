using System;
using System.Diagnostics;

namespace MiniPricerKata
{
    [DebuggerDisplay("{Date.ToString(\"yyyy-MM-dd\")} {Value}")]
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
            return $"{Date:yyyy-MM-dd} {Value}";
        }
    }
}