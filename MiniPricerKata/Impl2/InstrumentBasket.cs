using System.Collections.Generic;
using System.Linq;

namespace MiniPricerKata.Impl2
{
    public class InstrumentBasket : Basket
    {
        public Correlation Correlation { get; }
        public IEnumerable<BasketItem> BasketItems { get; }

        public InstrumentBasket(Correlation correlation, IEnumerable<BasketItem> basketItems)
        {
            Correlation = correlation;
            BasketItems = basketItems;
        }

        public bool IsEmpty()
        {
            return BasketItems.Any();
        }
    }

    public interface Basket
    {
        bool IsEmpty();

        IEnumerable<BasketItem> BasketItems { get; }

        Correlation Correlation { get; }
    }

    public class EmptyBasket : Basket
    {
        public bool IsEmpty()
        {
            return true;
        }

        public IEnumerable<BasketItem> BasketItems => Enumerable.Empty<BasketItem>();
        public Correlation Correlation { get; }
    }
}