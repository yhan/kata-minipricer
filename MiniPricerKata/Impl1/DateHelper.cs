using System;

namespace MiniPricerKata.Impl1
{
    public class DateHelper
    {
        public static DateTime Make(int year, int month, int day)
        {
            return new DateTime(year, month, day);
        }
    }
}