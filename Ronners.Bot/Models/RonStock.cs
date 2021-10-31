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

        public RonStock()
        {
            Change = 0;
        }

        public override string  ToString()
        {
            return string.Format("{0} ({1}) {2}rp",CompanyName,Symbol,Price);
        }
    }
}
