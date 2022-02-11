namespace MarketData.Gateway.Models
{
    public class ContributionRequest
    {
        public MarketDataType MarketDataType { get; set; }
        public FxQuote FxQuote { get; set; }
    }
}