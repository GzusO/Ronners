using System;

namespace Ronners.Loot
{
    public class Affix
    {
        protected string Name{get;set;}
        public string Modifer{get;set;}
        protected int MinValue{get;set;}
        protected int MaxValue{get;set;}
        public int Value{get;set;}

        private Random rand {get;set;}
        public Affix(string data)
        {
            rand = new Random();
            string[] dataPieces = data.Split(',');
            if(dataPieces.Length != 4)
                throw new Exception("Bad Data - Invalid Number of Args");
            Name = dataPieces[0];
            Modifer = dataPieces[1];
            int temp;
            if(Int32.TryParse(dataPieces[2],out temp))
            {
                MinValue = temp;
            }
            else
                throw new Exception("Bad Data - Can't Parse to Int");
            if(Int32.TryParse(dataPieces[3], out temp))
            {
                MaxValue = temp;
            }
            else
                throw new Exception("Bad Data - Can't Parse to Int");
        }

        public Affix(Affix affix)
        {
            rand = new Random();
            Name = affix.Name;
            Modifer = affix.Modifer;
            MinValue = affix.MinValue;
            MaxValue = affix.MaxValue;
            Value = rand.Next(MinValue,MaxValue+1);
        }

        public string BonusString()
        {
            return $"{Modifer}: +{Value}";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}