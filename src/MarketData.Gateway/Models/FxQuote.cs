namespace MarketData.Gateway.Models
{
    public class FxQuote : MarketData
    {
        public Currency Currency { get; set; }
        public float Bid { get; set; }
        public float Ask { get; set; }
    }
}