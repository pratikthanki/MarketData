namespace MarketData.Gateway.Models
{
    public class FxQuote : MarketData
    {
        public Currency Currency { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }
}