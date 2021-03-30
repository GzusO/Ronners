using System;

namespace Ronners.Loot
{
    public class Monster
    {
        public string Class{get;set;}
        public string Type{get;set;}
        public int Level{get;set;}
        public string TreasureClass{get;set;}

        public Monster(string data)
        {
            string[] dataPieces = data.Split(',');
            if(dataPieces.Length != 4)
                throw new Exception("Bad Data - Invalid Number of Args");
            Class = dataPieces[0];
            Type = dataPieces[1];
            int temp;
            if(Int32.TryParse(dataPieces[2],out temp))
            {
                Level = temp;
            }
            else
                throw new Exception("Bad Data - Can't Parse to Int");
            TreasureClass = dataPieces[3];
        }
    }
}