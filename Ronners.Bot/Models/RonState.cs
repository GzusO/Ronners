using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Ronners.Bot.Services;

namespace Ronners.Bot.Models
{
    [Table("ronstate")]
    public class RonState
    {
        public int MaxHealth{get;set;}
        public int MaxHappiness{get;set;}
        public int MaxHunger{get;set;}
        
        private int _health;
        public int Health 
        {
            get
            {
                return _health;
            }
            set
            {
                if (value > MaxHealth)
                    _health = MaxHealth;
                else if (value < 0)
                    _health= 0;
                else
                    _health = value;
            }
        }

        private int _hunger;
        public int Hunger
        {
            get
            {
                return _hunger;
            }
            set
            {
                if (value > MaxHunger)
                    _hunger = MaxHunger;
                else if (value < 0)
                    _hunger= 0;
                else
                    _hunger = value;
            }
        }    

        private int _happiness;    
        public int Happiness
        {
            get
            {
                return _happiness;
            }
            set
            {
                if (value > MaxHappiness)
                    _happiness = MaxHappiness;
                else if (value < 0)
                    _happiness= 0;
                else
                    _happiness = value;
            }
        }
        public int Ronners {get;set;}      //Meta/Luck
        public int Objectivity {get;set;}  //Wisdom 
        public int Normalcy {get;set;}     //Charisma
        public int Nutrition {get;set;}    //Constitution
        public int Erudition {get;set;}    //Intelligence
        public int Rapidity {get;set;}     //Dexterity
        public int Strength {get;set;}     //Strength
        
        public int Level {get;set;}
        public int Experience{get;set;}
        public int SkillPoints{get;set;}
        public int TalentPoints{get;set;}
        public bool CanSustain{get;set;}
        public Dictionary<string,bool> Talents{get;set;} = new Dictionary<string, bool>();

        
        [JsonIgnore] public int NextLevel
        {
            get
            {
                return (Level) switch{
                    (<=0)=>100,
                    (<=198)=>100+(Level*50),
                    _=>10000
                };
            }
        }
        [JsonIgnore] public string CurrentActivity{get;set;} ="Wandering aimlessly.";
        [JsonIgnore] public double XpMultiplier {get{return (Happiness+Hunger+Health)/150.0;}}
        [JsonIgnore] public bool IsHungry{get{return Hunger+(MaxHunger/10)<MaxHunger;}}
        [JsonIgnore] public bool HasSpace{get{return false;}}

        private string _filePath;
        public RonState()
        {
            //Required By Dapper
        }
        public RonState(string file)
        {
            _filePath = file;
        }

        internal void ApplyTalentBonuses(List<Bonus> bonuses)
        {
            foreach (var bonus in bonuses)
            {
                switch (bonus.Type)
                {
                    case BonusType.Health:    
                        MaxHealth+=(int)bonus.Value;
                        break;
                    case BonusType.Happiness:
                        MaxHappiness+=(int)bonus.Value;
                        break;
                    case BonusType.Hunger:
                        MaxHunger+=(int)bonus.Value;
                        break;
                    case BonusType.SelfSustaining:
                        CanSustain = true;
                        break;
                    default:
                        break;
                };
            }
        }

        public async Task SaveAsync(string fileName)
        {
            try
            {
                var json = JsonSerializer.Serialize(this,new JsonSerializerOptions(){WriteIndented =true});
                await File.WriteAllTextAsync(fileName,json,new UTF8Encoding(false));
            }
            catch (System.IO.IOException e)
            {
                await LoggingService.LogAsync("bot",Discord.LogSeverity.Error,"Failed to save",e);
            }
        }
        public static RonState Load(string fileName)
        {
            if(!File.Exists(fileName))
                return new RonState();

            var json = File.ReadAllText(fileName,new UTF8Encoding(false));
            return JsonSerializer.Deserialize<RonState>(json);

        }
    }
}

