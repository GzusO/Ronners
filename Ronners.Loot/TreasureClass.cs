using System;

namespace Ronners.Loot
{
    public class TreasureClass
    {
        public string Name{get;set;}
        public string Item1{get;set;}
        public string Item2{get;set;}
        public string Item3{get;set;}

        public TreasureClass(string data)
        {
            string[] dataPieces = data.Split(',');
            if(dataPieces.Length != 4)
                throw new Exception("Bad Data - Invalid Number of Args");
            Name = dataPieces[0];
            Item1 = dataPieces[1];
            Item2 = dataPieces[2];
            Item3 = dataPieces[3];

        }
    }
}