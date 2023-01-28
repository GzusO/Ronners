using System.Text.Json.Serialization;

namespace Ronners.Bot.Models
{    
  public class Item    {

        public int ItemID{get;set;}
        public string Name{get;set;}
        public string Collection{get;set;}
        public Rarity Rarity{get;set;}
        public string Description{get;set;}
        public Item()
        {

        }

        public int RarityToWeight()
        {
          switch(Rarity)
          {
            case Rarity.Common:
              return 15;
            case Rarity.Uncommon:
              return 10;
            case Rarity.Rare:
              return 8;
            case Rarity.SuperRare:
              return 5;
            case Rarity.UltraRare:
              return 1;
            default:
              return 1;
          }
        }
        public string RarityName()
        {
          switch(Rarity)
          {
            case Rarity.Common:
              return "Common";
            case Rarity.Uncommon:
              return "Uncommon";
            case Rarity.Rare:
              return "Rare";
            case Rarity.SuperRare:
              return "Super Rare";
            case Rarity.UltraRare:
              return "Ultra Rare";
            default:
              return "Unknown";
          }
        }

        public override string ToString()
        {
            return $"{Name} ({this.RarityName()})";
        }


    }
    public enum Rarity {
            Common,
            Uncommon,
            Rare,
            SuperRare,
            UltraRare
        }
}
