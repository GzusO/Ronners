using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    public class Fish
    {
        public int FishID{get;set;}
        public string Emoji {get;set;}
        public string Name {get;set;}
        public double Length {get;set;}//cm
        public double Weight {get;set;}//kg

        public Fish()
        {
            //Required By Dapper
        }

        public Fish(int id, double weight, double length)
        {
            FishID =id;
            Emoji = GetEmojiByID(id);
            Length= length;
            Weight = weight;
        }

        //":fish:",":tropical_fish:",":blowfish:",":shark:",":shrimp:"
        private string GetEmojiByID(int id)
        {
            switch (id)
            {
                case 1:
                    return ":fish:";
                case 2:
                    return ":tropical_fish:";
                case 3:
                    return ":shark:";
                case 4:
                    return ":blowfish:";
                case 5:
                    return ":shrimp:";
                default:
                    return "";
            }
        }
    }

    public class UserFish
    {
        public ulong UserID{get;set;}
        public Fish Fish{get;set;}
    }
}

