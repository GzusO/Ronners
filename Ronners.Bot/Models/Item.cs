using System;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models
{
    public class Item : IEquatable<Item>
    {

        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Collection { get; set; }
        public Rarity Rarity { get; set; }
        public string Description { get; set; }
        public Item()
        {

        }

        public int RarityToWeight()
        {
            switch (Rarity)
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
            switch (Rarity)
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

        public override bool Equals(object obj) => this.Equals(obj as Item);

        public bool Equals(Item item)
        {
            if (item is null)
                return false;

            if (Object.ReferenceEquals(this, item))
                return true;

            if (this.GetType() != item.GetType())
                return false;

            return (ItemID == item.ItemID);
        }

        public override int GetHashCode() => (ItemID).GetHashCode();
        public static bool operator ==(Item lhs, Item rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Item lhs, Item rhs) => !(lhs == rhs);
    }
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        SuperRare,
        UltraRare
    }
}
