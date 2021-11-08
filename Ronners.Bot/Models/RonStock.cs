using System.Text.Json.Serialization;

namespace Ronners.Bot.Models
{    
  public class RonStock    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } 

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } 

        [JsonPropertyName("price")]
        public int Price {get; set;}

        [JsonPropertyName("change")]
        public int Change {get;set;}

        [JsonPropertyName("volatility")]
        public double Volatility {get;set;}

        [JsonPropertyName("min")]
        public int Min {get;set;}

        [JsonPropertyName("max")]
        public int Max {get;set;}

        [JsonPropertyName("spread")]
        public double Spread {get;set;}

        [JsonPropertyName("shift")]
        public double Shift {get;set;}

        [JsonPropertyName("increment")]
        public long Increment {get;set;}

        [JsonIgnore]
        public int Average {get{return (Min+Max)/2;}}

        public RonStock(string symbol, string company, int min, int max, double spread, double volatility, double shift = 0, long increment = 0)
        {
            Symbol = symbol;
            CompanyName = company;
            Min=min;
            Max=max;
            Spread = spread;
            Volatility = volatility;
            Shift = shift;
            Increment = increment;
            Change = 0;
            Price = (Min+Max)/2;
        }
        public RonStock()
        {}

        public override string  ToString()
        {
            return string.Format("{0} ({1}) {2}rp",CompanyName,Symbol,Price);
        }
    }
}
