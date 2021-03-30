using System;
using System.Collections.Generic;

namespace Ronners.Loot
{
    public class Armor : Item
    {
        public Armor(string data) : base()
        {
            string[] dataPieces = data.Split(',');
            if(dataPieces.Length != 3)
                throw new Exception("Bad Data - Invalid Number of Args");
            Name = dataPieces[0];
            int temp;
            if(Int32.TryParse(dataPieces[1],out temp))
            {
                MinValue = temp;
            }
            else
                throw new Exception("Bad Data - Can't Parse to Int");
            if(Int32.TryParse(dataPieces[2], out temp))
            {
                MaxValue = temp;
            }
            else
                throw new Exception("Bad Data - Can't Parse to Int");
        }

        public static Armor DeepCopy(Armor armor)
        {
            Armor other = armor.MemberwiseClone() as Armor;
            other.Suffixes = new List<Suffix>();
            other.Prefixes = new List<Prefix>();
            return other; 
        }
        public string GetMod()
        {
            return $"Armor";
        }
        public override string ToString()
        {
            string result = $"{base.ToString()}";
            return result;
        }
    }
}