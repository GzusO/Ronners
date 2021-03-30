using System.Text.Json.Serialization;

namespace Ronners.Bot.Models
{    
  public class StockQuoteIEX    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } 

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } 

        [JsonPropertyName("primaryExchange")]
        public string PrimaryExchange { get; set; } 

        [JsonPropertyName("calculationPrice")]
        public string CalculationPrice { get; set; } 

        [JsonPropertyName("open")]
        public double? Open { get; set; } 

        [JsonPropertyName("openTime")]
        public double? OpenTime { get; set; } 

        [JsonPropertyName("openSource")]
        public string OpenSource { get; set; } 

        [JsonPropertyName("close")]
        public double? Close { get; set; } 

        [JsonPropertyName("closeTime")]
        public double? CloseTime { get; set; } 

        [JsonPropertyName("closeSource")]
        public string CloseSource { get; set; } 

        [JsonPropertyName("high")]
        public double? High { get; set; } 

        [JsonPropertyName("highTime")]
        public double? HighTime { get; set; } 

        [JsonPropertyName("highSource")]
        public string HighSource { get; set; } 

        [JsonPropertyName("low")]
        public double? Low { get; set; } 

        [JsonPropertyName("lowTime")]
        public double? LowTime { get; set; } 

        [JsonPropertyName("lowSource")]
        public string LowSource { get; set; } 

        [JsonPropertyName("latestPrice")]
        public double? LatestPrice { get; set; } 

        [JsonPropertyName("latestSource")]
        public string LatestSource { get; set; } 

        [JsonPropertyName("latestTime")]
        public string LatestTime { get; set; } 

        [JsonPropertyName("latestUpdate")]
        public double? LatestUpdate { get; set; } 

        [JsonPropertyName("latestVolume")]
        public double? LatestVolume { get; set; } 

        [JsonPropertyName("iexRealtimePrice")]
        public double? IexRealtimePrice { get; set; } 

        [JsonPropertyName("iexRealtimeSize")]
        public double? IexRealtimeSize { get; set; } 

        [JsonPropertyName("iexLastUpdated")]
        public double? IexLastUpdated { get; set; } 

        [JsonPropertyName("delayedPrice")]
        public double? DelayedPrice { get; set; } 

        [JsonPropertyName("delayedPriceTime")]
        public double? DelayedPriceTime { get; set; } 

        [JsonPropertyName("oddLotDelayedPrice")]
        public double? OddLotDelayedPrice { get; set; } 

        [JsonPropertyName("oddLotDelayedPriceTime")]
        public double? OddLotDelayedPriceTime { get; set; } 

        [JsonPropertyName("extendedPrice")]
        public double? ExtendedPrice { get; set; } 

        [JsonPropertyName("extendedChange")]
        public double? ExtendedChange { get; set; } 

        [JsonPropertyName("extendedChangePercent")]
        public double? ExtendedChangePercent { get; set; } 

        [JsonPropertyName("extendedPriceTime")]
        public double? ExtendedPriceTime { get; set; } 

        [JsonPropertyName("previousClose")]
        public double? PreviousClose { get; set; } 

        [JsonPropertyName("previousVolume")]
        public double? PreviousVolume { get; set; } 

        [JsonPropertyName("change")]
        public double? Change { get; set; } 

        [JsonPropertyName("changePercent")]
        public double? ChangePercent { get; set; } 

        [JsonPropertyName("volume")]
        public double? Volume { get; set; } 

        [JsonPropertyName("iexMarketPercent")]
        public double? IexMarketPercent { get; set; } 

        [JsonPropertyName("iexVolume")]
        public double? IexVolume { get; set; } 

        [JsonPropertyName("avgTotalVolume")]
        public double? AvgTotalVolume { get; set; } 

        [JsonPropertyName("iexBidPrice")]
        public double? IexBidPrice { get; set; } 

        [JsonPropertyName("iexBidSize")]
        public double? IexBidSize { get; set; } 

        [JsonPropertyName("iexAskPrice")]
        public double? IexAskPrice { get; set; } 

        [JsonPropertyName("iexAskSize")]
        public double? IexAskSize { get; set; } 

        [JsonPropertyName("iexOpen")]
        public double? IexOpen { get; set; } 

        [JsonPropertyName("iexOpenTime")]
        public double? IexOpenTime { get; set; } 

        [JsonPropertyName("iexClose")]
        public double? IexClose { get; set; } 

        [JsonPropertyName("iexCloseTime")]
        public double? IexCloseTime { get; set; } 

        [JsonPropertyName("marketCap")]
        public double? MarketCap { get; set; } 

        [JsonPropertyName("peRatio")]
        public double? PeRatio { get; set; } 

        [JsonPropertyName("week52High")]
        public double? Week52High { get; set; } 

        [JsonPropertyName("week52Low")]
        public double? Week52Low { get; set; } 

        [JsonPropertyName("ytdChange")]
        public double? YtdChange { get; set; } 

        [JsonPropertyName("lastTradeTime")]
        public double? LastTradeTime { get; set; } 

        [JsonPropertyName("isUSMarketOpen")]
        public bool? IsUSMarketOpen { get; set; } 

        public override string  ToString()
        {
            return string.Format("{0} ({1}) ${2} \n{3} ({4}%)",CompanyName,Symbol,LatestPrice,Change,ChangePercent);
        }
    }
}
