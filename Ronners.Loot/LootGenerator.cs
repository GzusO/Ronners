using System;
using System.Collections.Generic;
using System.IO;

namespace Ronners.Loot
{
    public class LootGenerator
    {
        public List<Monster> Monsters{get;set;}
        public List<TreasureClass> TreasureClasses{get;set;}
        public List<Item> Items{get;set;}
        public List<Prefix> Prefixes{get;set;}
        public List<Suffix> Suffixes{get;set;}

        private Random rand;

        public LootGenerator()
        {
            Monsters = new List<Monster>();
            TreasureClasses = new List<TreasureClass>();
            Items = new List<Item>();
            Prefixes = new List<Prefix>();
            Suffixes = new List<Suffix>();
            rand = new Random();
        }
        public LootGenerator(string dataFolder)
        {
            Monsters = LoadMonsters(Path.Combine(dataFolder,"monsters.txt"));
            TreasureClasses = LoadTreasureClasses(Path.Combine(dataFolder,"TreasureClasses.txt"));
            Items = LoadArmor(Path.Combine(dataFolder,"Armor.txt"));
            Items.AddRange(LoadWeapons(Path.Combine(dataFolder,"Weapons.txt")));
            Prefixes = LoadPrefixes(Path.Combine(dataFolder,"MagicPrefix.txt"));
            Suffixes = LoadSuffixes(Path.Combine(dataFolder,"MagicSuffix.txt"));
            rand = new Random();
        }

        public Item Generate()
        {
            Monster monster = Monsters[rand.Next(Monsters.Count)];
            Item item = ResolveItem(monster.TreasureClass);
            Item newItem;
            if(item is Weapon)
                newItem = Weapon.DeepCopy(item as Weapon);
            else if(item is Armor)
                newItem = Armor.DeepCopy(item as Armor);
            else
                return new Item(); 
            if(rand.Next(2)==1)
                newItem.AddPrefix(Prefix.GeneratePrefix(Prefixes[rand.Next(Prefixes.Count)]));
            if(rand.Next(2)==1)
                newItem.AddSuffix(Suffix.GenerateSuffix(Suffixes[rand.Next(Suffixes.Count)]));

            newItem.RandomizeValue();

            return newItem;
            /*
            if(newItem is Weapon)
                return ((Weapon)newItem).ToString();
            else if(newItem is Armor)
                return ((Armor)newItem).ToString();
            else
                return newItem.ToString();
            */
            
        }

        private Item ResolveItem(string name)
        {
            int choice;
            while(TreasureClasses.Exists(x => x.Name == name))
            {
                choice = rand.Next(3);
                var tc = TreasureClasses.Find(x=>x.Name == name);
                if(choice == 0)
                    name = tc.Item1;
                else if(choice == 1)
                    name = tc.Item2;
                else
                    name = tc.Item3;
            }
            return Items.Find(x => x.Name == name);
        }

        private List<Suffix> LoadSuffixes(string dataFile)
        {
            List<Suffix> result = new List<Suffix>();
            foreach(string line in File.ReadLines(dataFile))
            {
                result.Add(new Suffix(line));
            }
            return result;
        }

        private List<Prefix> LoadPrefixes(string dataFile)
        {
            List<Prefix> result = new List<Prefix>();
            foreach(string line in File.ReadLines(dataFile))
            {
                result.Add(new Prefix(line));
            }
            return result;
        }

        private IEnumerable<Item> LoadWeapons(string dataFile)
        {
            List<Weapon> result = new List<Weapon>();
            foreach(string line in File.ReadLines(dataFile))
            {
                result.Add(new Weapon(line));
            }
            return result;
        }

        private List<Item> LoadArmor(string dataFile)
        {
            List<Item> result = new List<Item>();
            foreach(string line in File.ReadLines(dataFile))
            {
                result.Add(new Armor(line));
            }
            return result;
        }

        private List<TreasureClass> LoadTreasureClasses(string dataFile)
        {
            List<TreasureClass> result = new List<TreasureClass>();
            foreach(string line in File.ReadLines(dataFile))
            {
                result.Add(new TreasureClass(line));
            }
            return result;
        }

        private List<Monster> LoadMonsters(string dataFile)
        {
            List<Monster> result = new List<Monster>();
            foreach(string line in File.ReadLines(dataFile))
            {
                result.Add(new Monster(line));
            }
            return result;
        }
    }
}
