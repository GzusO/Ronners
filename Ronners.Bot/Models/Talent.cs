using System.Collections.Generic;
using System.ComponentModel;
using Ronners.Bot.Extensions;
namespace Ronners.Bot.Models
{
    public class Talent
    {
        public string TalentName{get;set;}
        public int RequiredRonners{get;set;}
        public int RequiredObjectivity{get;set;}
        public int RequiredNutrition{get;set;}
        public int RequiredNormalcy{get;set;}
        public int RequiredErudition{get;set;}
        public int RequiredRapidity{get;set;}
        public int RequiredStrength{get;set;}

        public List<string> RequiredTalents{get;set;}
        public List<Bonus> Bonuses{get;set;}

        public Talent(string talentName, int ronners=0, int objectivity=0, int nutrition=0, int normalcy=0, int erudition=0, int rapidity=0, int strength=0)
        {
            this.TalentName = talentName;
            this.RequiredRonners = ronners;
            this.RequiredObjectivity = objectivity;
            this.RequiredNutrition = nutrition;
            this.RequiredNormalcy = normalcy;
            this.RequiredErudition = erudition;
            this.RequiredRapidity = rapidity;
            this.RequiredStrength = strength;
            this.Bonuses = new List<Bonus>();
            this.RequiredTalents = new List<string>();
        }

        public Talent AddBonus(BonusType type, double value)
        {
            this.Bonuses.Add(new Bonus(type,value));
            return this;
        }

        public Talent AddRequiredTalent(string talentName)
        {
            this.RequiredTalents.Add(talentName);
            return this;
        }
    }


    public enum BonusType
    {
        
        [Description("Max Happiness")] Happiness,
        [Description("Max Health")] Health,
        [Description("Max Hunger")] Hunger,
        [Description("Inventory")] Inventory,
        [Description("Inventory Slots")]InventorySpace,
        [Description("Alphabetical Sorting")] AlphabeticalSort,
        [Description("Randomly finds food")] SelfSustaining 
    }

    public class Bonus
    {
        public BonusType Type{get;set;}
        public double Value{get;set;}

        public Bonus(BonusType type, double value)
        {
            this.Type=type;
            this.Value=value;
        }


        public override string ToString()
        {
            return $"+ {(Value==0 ? "" :Value)} {Type.GetEnumDescription()}";
        }
    }
}