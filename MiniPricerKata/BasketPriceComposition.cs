using System.Collections;
using System.Collections.Generic;

namespace MiniPricerKata
{
    public class BasketPriceComposition : IEnumerable
    {

        private readonly IDictionary<BasketItem, Price> _values = new Dictionary<BasketItem, Price>();

        public BasketPriceComposition(Basket basket, Price pivotPrice)
        {
            foreach (var basketBasketItem in basket.BasketItems)
            {
                Price price;
                if (basketBasketItem.IsPivot)
                {
                    price = pivotPrice;
                }
                else
                {
                    price = pivotPrice * basket.Correlation;
                }


                _values.Add(basketBasketItem, price);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public Price this[BasketItem basketItem] => _values[basketItem];
    }
}