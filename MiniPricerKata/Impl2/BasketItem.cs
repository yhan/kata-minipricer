namespace MiniPricerKata.Impl2
{
    public class BasketItem
    {
        public Instrument Instrument { get; }
        public bool IsPivot { get; }

        public BasketItem(Instrument instrument, bool isPivot)
        {
            Instrument = instrument;
            IsPivot = isPivot;
        }
    }
}