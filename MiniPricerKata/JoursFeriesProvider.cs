using System;
using System.Collections.Generic;

namespace MiniPricerKata
{
    public class JoursFeriesProvider : IProvideJoursFeries
    {
        private static readonly ISet<DateTime> _joursFeries = new HashSet<DateTime>
            {new DateTime(2019, 5, 1), new DateTime(2019, 5, 5), new DateTime(2019, 5, 8)};


        public bool IsJourFerie(DateTime date)
        {
            return _joursFeries.Contains(date);
        }
    }
}