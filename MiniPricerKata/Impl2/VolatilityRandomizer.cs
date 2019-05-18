﻿using System;

namespace MiniPricerKata.Impl2
{
    class VolatilityRandomizer : IRandomizeVolatility
    {
        private static Random _random = new Random();

        public double Randomrize(double volatility)
        {
            var next = _random.Next(3);
            int sign;
            switch ((PriceMoveTrend)next)
            {
                case PriceMoveTrend.Down:
                    sign = -1;
                    break;
                case PriceMoveTrend.Flat:
                    sign = 0;
                    break;
                case PriceMoveTrend.Up:
                    sign = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return volatility * sign;
        }
    }
}