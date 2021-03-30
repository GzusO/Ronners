using System;
using System.Collections.Generic;

namespace Ronners.Loot
{
    public class Item
    {
        public string Name{get;set;}
        public int MinValue{get;set;}
        public int MaxValue{get;set;}
        public int Value{get;set;}
        protected List<Prefix> Prefixes{get;set;}
        protected List<Suffix> Suffixes{get;set;}

        public Item()
        {
            Prefixes = new List<Prefix>();
            Suffixes = new List<Suffix>();
        }
        public void AddPrefix(Prefix prefix)
        {
            Prefixes.Add(prefix);
        }
        public void AddSuffix(Suffix suffix)
        {
            Suffixes.Add(suffix);
        }

        public void RandomizeValue()
        {
            var rand = new Random();
            Value = rand.Next(MinValue,MaxValue+1);
        }
        public override string ToString()
        {
            string result = "";
            foreach(var pre in Prefixes)
                result+=$" {pre} ";
            result+= $"{Name}";
            foreach(var suf in Suffixes)
                result+= $" {suf} ";
            return result.Trim();
        }

        public IEnumerable<Prefix> GetPrefixes()
        {
            return Prefixes;
        }
        public IEnumerable<Suffix> GetSuffixes()
        {
            return Suffixes;
        }

        public string GetAffixBonusString()
        {
            string result = "";
            foreach(var prefix in Prefixes)
                result+= $"{prefix.BonusString()}\n";
            foreach(var suffix in Suffixes)
                result+= $"{suffix.BonusString()}\n";
            return result.Trim();
        }
    }
}