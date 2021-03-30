using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("ideas")]
    public class Idea
    {
        [Key]
        public long? ideaid{get;set;}
        public string idea{get;set;}
        public int priority {get;set;}

        public Idea(string text, int val)
        {
            idea = text;
            priority = val;
        }

        public Idea()
        {
            //Required By Dapper
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}({2})",priority,idea,ideaid);
        }
    }
}

