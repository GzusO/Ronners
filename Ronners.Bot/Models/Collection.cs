using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    public class Collection
    {
        public string Name{get;set;}
        public int NumberOfItems{get;set;}

        public Collection()
        {
            //Required By Dapper
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

