using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Ronners.Bot.Extensions;
using Ronners.Bot.Models;

namespace Ronners.Bot.Services
{
    public class RonService
    {
        private readonly Random _rand;
        private readonly GameService _game;
        private readonly Discord.WebSocket.DiscordSocketClient _discord;
        public RonState state;
        public List<Talent> Talents;
        public int gameTick =0;
        public List<(ulong threadId, ulong guildId)> Threads;
        private string RonFilePath {get;set;} = "ron.json";

        public RonService(IServiceProvider services)
        {
            _rand = services.GetRequiredService<Random>();
            _game= services.GetRequiredService<GameService>();
            _discord = services.GetRequiredService<Discord.WebSocket.DiscordSocketClient>();
        }

        public async Task InitializeAsync()
        {
            state = RonState.Load(RonFilePath);
            Talents = InitializeTalents();
            foreach (var talent in Talents)
            {
                if(!state.Talents.ContainsKey(talent.TalentName))
                    state.Talents.Add(talent.TalentName,false);
            }

            Threads = new List<(ulong threadId, ulong guildId)>();
            Threads = (await _game.GetRonThreads()).ToList();

            await state.SaveAsync(RonFilePath);
        }

        public async Task SetDiscordState()
        {
            await _discord.SetGameAsync($"*Lvl: {state.Level}* Health:{state.Health}/{state.MaxHealth} Hunger:{state.Hunger}/{state.MaxHunger} Happiness:{state.Happiness}/{state.MaxHappiness}.",null,Discord.ActivityType.Playing);
        }

        public async void UpdateRon(object _)
        {
            gameTick++;
            gameTick %= 60;
            await LoggingService.LogAsync("bot",Discord.LogSeverity.Info,$"Game tick: {gameTick}");
            if(gameTick == 0)
            {
                await ReturnPoints();

                await LoggingService.LogAsync("bot",Discord.LogSeverity.Info,$"Update tick");
                if(state.Health < 75)
                {
                    state.Happiness--;
                }
                if(state.Hunger <50)
                {
                    state.Health--;
                    state.Happiness--;
                }
                state.Hunger--;
                state.Happiness -= _rand.Next(3);
                if(state.Hunger < 0)
                    state.Hunger = 0;
                if(state.Happiness < 0)
                    state.Happiness = 0;
                if(state.Health < 0)
                    state.Health = 0;
            }

            //Try to do random event.
            RandomEvent();

            state.Experience+= (int)(state.XpMultiplier*2);

            await state.SaveAsync(RonFilePath);
            await SetDiscordState();
        }

        private async Task ReturnPoints()
        {
            var gifts = await _game.GetRonGiftsNotReturned();
            long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach(var gift in gifts.Where(x=> x.ReturnDate <= time))
            {
                await _game.ReturnGift(gift.RonGiftID);

                var user = await _discord.GetUserAsync(gift.UserID);
                await _game.AddRonPoints(user, gift.ReturnPoints);
                try
                {
                    await user.SendMessageAsync($"Ronners has return to you {gift.ReturnPoints} RonPoints.");
                }
                catch(Exception e)
                {

                }
            }
        }
        private async void RandomEvent()
        {
            var randomEvent = _rand.Next(0,180);
            string Event = "";
            switch(randomEvent)
            {
                case 83:
                case 84:
                case 85:
                    Event +="Ronners wandered aimlessly.";
                    break;
                case  86:
                    Event +="Ronners stared up at the stars.";
                    break;
                case  87:
                    Event += "Ronners tripped on a rock.\n";
                    var dmg = _rand.Next(5);
                    dmg -= 2;
                    dmg = dmg <= 0 ? 0 : dmg;
                    Event += $"Ronners lost {dmg} health.";
                    state.Health-= dmg;
                    break;
                case 88:
                    Event += "Ronners found some flowers.\n";
                    var flower  = _rand.Next(0,Math.Max(10-state.Erudition,2));
                    if(flower == 0)
                    {
                        Event += "Ronners smelled the flowers.\n";
                        Event += "Ronners gained 1 happiness.";
                        state.Happiness++;
                    }
                    else if(flower == 1 && state.HasSpace)
                    {
                        Event += "Ronners picked the flowers.";
                        //TODO: Add Flowers to inventory
                    }
                    else if(state.CanSustain && state.IsHungry)
                        Event += "Ronners ate the flowers.";
                    else
                        Event += "Ronners looked at the flowers.";
                        
                    break;
                case 89:
                    Event = "Ronners found an apple.\n";
                    if(state.CanSustain && state.IsHungry)
                    {
                        Event+="Ronners ate the apple gaining 1 hunger.";
                        state.Hunger++;
                    }
                    else
                    {
                        Event+="Ronners threw the apple.\n";
                        var hit = _rand.Next(0,3);
                        var xp = 5*hit;
                        if(hit == 2)
                            Event+="Ronners hit a tree with the apple.\n";
                        else if(hit == 1)
                            Event+="Ronners hit a rock with the apple.\n";
                        else
                            Event+="Ronners hit nothing with the apple.";
                        if(xp > 0)
                            Event+=$"Ronners gained {xp} xp.";
                        state.Experience+= hit*5;
                    }
                    break;
                default:
                    break;
            }
            await state.SaveAsync(RonFilePath);
            if(string.IsNullOrWhiteSpace(Event))
                return;
            await LoggingService.LogAsync("bot",LogSeverity.Info, "Event Happened");
            await UpdateThreads(GetEventEmbed(Event));
        }

        private async Task UpdateThreads(Embed message)
        {
            foreach(var thread in Threads)
            {
                ITextChannel channel = await _discord.GetChannelAsync(thread.threadId) as ITextChannel;
                await channel.SendMessageAsync("",false,message);
            }
        }

        public async Task<string> FeedRon()
        {
            var values =Enum.GetValues(typeof(Food));
            Food food = (Food)values.GetValue(_rand.Next(values.Length));
            state.Hunger+= 10+(int)food;

            await state.SaveAsync(RonFilePath);
            return food.GetEnumDescription();
        }

        public async Task<string> PlayWithRon()
        {
            var values =Enum.GetValues(typeof(Ronners.Bot.Models.Game));
            Ronners.Bot.Models.Game game = (Ronners.Bot.Models.Game)values.GetValue(_rand.Next(values.Length));
            state.Happiness+= 5+(int)game;

            await state.SaveAsync(RonFilePath);
            return game.GetEnumDescription();
        }

        public async Task HealRon()
        {
            state.Health = state.MaxHealth;
            await state.SaveAsync(RonFilePath);
        }
        
        public async Task<int> LevelUp()
        {
            //Have enough XP to level up
            if(state.Experience >= state.NextLevel)
            {
                state.Experience-=state.NextLevel;
                state.Level++;
                state.SkillPoints++;
                if(state.Level % 5 == 0)
                    state.TalentPoints++;
                await state.SaveAsync(RonFilePath);
                return state.Level;
            }
            else
            {
                return -1*(state.NextLevel-state.Experience);
            }
        }

        internal async Task<bool> AddTalent(string talent)
        {
            if(!CanAfford(talent))
                return false;
            if(!CanAcquire(talent, out _))
                return false;
            
            state.TalentPoints--;

            //Mark Talent
            state.Talents[talent]=true;
            state.ApplyTalentBonuses(Talents.Find(x=> x.TalentName==talent).Bonuses);
            
            await state.SaveAsync(RonFilePath);
            return true;
        }

        public async Task AddThread(ulong id, ulong guildId)
        {
            Threads.Add((id,guildId));
            await _game.AddRonThread(id,guildId);
        }

        public async Task AddTalentPoints(int amount)
        {
            state.TalentPoints+= amount;
            await state.SaveAsync(RonFilePath);
        }

        public async Task<int> SkillUp(Skill skill)
        {
            int level = 0;
            if(state.SkillPoints > 0)
            {
                state.SkillPoints--;
                switch (skill)
                {
                    case Skill.Ronners:
                        state.Ronners++;
                        level = state.Ronners;
                        break;
                    case Skill.Objectivity:
                        state.Objectivity++;
                        level = state.Objectivity;
                        break;
                    case Skill.Nutrition:
                        state.Nutrition++;
                        level = state.Nutrition;
                        break;
                    case Skill.Normalcy:
                        state.Normalcy++;
                        level = state.Normalcy;
                        break;
                    case Skill.Erudition:
                        state.Erudition++;
                        level = state.Erudition;
                        break;
                    case Skill.Rapidity:
                        state.Rapidity++;
                        level = state.Rapidity;
                        break;
                    case Skill.Strength:
                        state.Strength++;
                        level = state.Strength;
                        break;
                }
            }
            await state.SaveAsync(RonFilePath);
            return level;
        }

        internal Embed GetTalentEmbed(string talentName)
        {
            var talent = Talents.Find(x=> x.TalentName==talentName);

            var requiredTalents = String.Join(", ",talent.RequiredTalents);
            var BonusString = "";
            foreach (var bonus in talent.Bonuses)
            {
                BonusString+=$"`{bonus.ToString()}`\n";
            }

            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(talent.TalentName);
            if(talent.RequiredRonners+talent.RequiredObjectivity+talent.RequiredNutrition+talent.RequiredNormalcy+talent.RequiredErudition+talent.RequiredRapidity+talent.RequiredStrength > 0)
                builder.AddField("Required Skills",RequiredSkillString(talent),false);
            if(talent.RequiredTalents.Count>0)
                builder.AddField("Required Talents",$"{requiredTalents}",false);
            builder.WithDescription(BonusString);
            builder.WithCurrentTimestamp();
            builder.WithColor(0,255,0);
            return builder.Build();
        }

        internal Embed GetTalentListEmbed(int page)
        {
            var pageSize = 50;
            var talents = Talents.Skip((page-1)*pageSize).Take(pageSize);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithCurrentTimestamp();
            builder.WithTitle($"Talents ({page}/{(Talents.Count+pageSize-1)/pageSize})");
            builder.WithDescription(string.Join('\n',talents.Select(x=> $"`[{(state.Talents[x.TalentName]?'X':'_')}] {x.TalentName}`")));

            return builder.Build();
        }

        internal Embed GetEventEmbed(string message)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithCurrentTimestamp();
            builder.WithTitle("Events");
            builder.WithDescription(message);
            builder.WithColor(Color.DarkPurple);
            return builder.Build();
        }

        internal string RequiredSkillString(Talent talent)
        {
            var sb = new StringBuilder();
            sb.Append(talent.RequiredRonners     > 0 ? $"Ronners: {talent.RequiredRonners}    ":"");
            sb.Append(talent.RequiredObjectivity > 0 ? $"Objectivity: {talent.RequiredObjectivity}    ":"");
            sb.Append(talent.RequiredNutrition   > 0 ? $"Nutrition: {talent.RequiredNutrition}    ":"");
            sb.Append(talent.RequiredNormalcy    > 0 ? $"Normalcy: {talent.RequiredNormalcy}    ":"");
            sb.Append(talent.RequiredErudition   > 0 ? $"Erudition: {talent.RequiredErudition}    ":"");
            sb.Append(talent.RequiredRapidity    > 0 ? $"Rapidity: {talent.RequiredRapidity}    ":"");
            sb.Append(talent.RequiredStrength    > 0 ? $"Strength: {talent.RequiredStrength}    ":"");
            return sb.ToString();
        }

        public bool TalentExists(string talent)
        {
            return Talents.Any(x=> x.TalentName == talent);
        }

        public Embed GetStateEmbed()
        {            
            int ticks = 30; //Should be even
            int halfTicks = ticks/2;

            double percentToLevel = Math.Min((double)state.Experience/(double)state.NextLevel,1.0);
            int xpTicks = (int)(ticks*percentToLevel);
            string leftTicks = new string('=',Math.Min(xpTicks,halfTicks)).PadRight(halfTicks,'-');
            string rightTicks = new string('=',Math.Max(xpTicks-halfTicks,0)).PadRight(halfTicks,'-');

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Ronners Stats");
            builder.AddField($"Level (SP: {state.SkillPoints} TP: {state.TalentPoints})",$"{state.Level}",false);
            builder.AddField("Experience",$"`[{leftTicks}{state.Experience}/{state.NextLevel}{rightTicks}]`",false);
            builder.AddField("Happiness",$"{state.Happiness}/{state.MaxHappiness}",true);
            builder.AddField("Health",$"{state.Health}/{state.MaxHealth}",true);
            builder.AddField("Hunger",$"{state.Hunger}/{state.MaxHunger}",true);

            builder.AddField($"Ronners",$"{state.Ronners}",false);
            builder.AddField($"Objectivity",$"{state.Objectivity}",false);
            builder.AddField($"Nutrition",$"{state.Nutrition}",false);
            builder.AddField($"Normalcy",$"{state.Normalcy}",false);
            builder.AddField($"Erudition",$"{state.Erudition}",false);
            builder.AddField($"Rapidity",$"{state.Rapidity}",false);
            builder.AddField($"Strength",$"{state.Strength}",false);

            builder.WithDescription(state.CurrentActivity);
            builder.WithCurrentTimestamp();
            builder.WithColor(state.Happiness,state.Health,state.Hunger);
            return builder.Build();
        }



        internal bool CanAcquire(string talentName, out string prereqs)
        {
            prereqs = "";
            var talent = Talents.Find(x=>x.TalentName==talentName);

            var canAcquire = true;
            foreach (var requiredTalent in talent.RequiredTalents)
            {
                if(!state.Talents.GetValueOrDefault(requiredTalent,false))
                {
                    canAcquire = false;
                    prereqs+=$"{requiredTalent} , ";
                }
            }
            if(state.Talents[talentName])
            {
                canAcquire = false;
                prereqs = "Already Have Talent";
            }
            return canAcquire;
        }

        internal bool CanAfford(string talent)
        {
            return state.TalentPoints>=1;
        }

        public bool GuildAlreadyHasThread(ulong guildId)
        {
            return Threads.Any(x=> x.guildId==guildId);
        }

        public Embed GetProfessionEmbed()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Ronners Professions");

            builder.AddField("Eccentric Billionaire","0/99",true);
            builder.AddField("CEO","0/99",true);
            builder.AddField("Crypto Day Trader","0/99",true);
            builder.AddField("Gamer","0/99",true);
            builder.AddField("Gambler","0/99",true);
            builder.AddField("Chess Player","0/99",true);
            builder.AddField("E-Girl","0/99",true);
            builder.AddField("Eater","0/99",true);
            builder.AddField("Gravy Saleman","0/99",true);
            builder.AddField("Astronaut","0/99",true);
            builder.AddField("Alchemist","0/99",true);
            builder.AddField("Archeologist","0/99",true);
            builder.AddField("Pizza Man","0/99",true);
            builder.AddField("Fisher","0/99",true);
            builder.AddField("Ring Wrangler","0/99",true);
            builder.AddField("Roofer","0/99",true);
            builder.AddField("Ronners","0/99",true);
            builder.AddField("Primer","0/99",true);
            builder.AddField("Wanderer","0/99",true);
            builder.AddField("Wetworker","0/99",true);
            builder.AddField("Joker","0/99",true);
            builder.AddField("Butler","0/99",true);
            builder.AddField("Beekeeper","0/99",true);
            builder.AddField("Bus Driver","0/99",true);
                                                                       

            builder.WithCurrentTimestamp();
            builder.WithColor(255,0,255);
            return builder.Build();
        }

        private double XpMultiplier()
        {
            return (state.Health + state.Hunger + state.Happiness)/150.0;
        }

        private List<Talent> InitializeTalents()
        {
            var talents = new List<Talent>();
            
            talents.Add(new Talent("Ronners",ronners:1)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d));
            
            talents.Add(new Talent("Ronners!",ronners:10)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners"));

            talents.Add(new Talent("Ronners!!",ronners:20)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!"));

            talents.Add(new Talent("Ronners!!!",ronners:30)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!"));

            talents.Add(new Talent("Ronners!!!!",ronners:40)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!!"));

            talents.Add(new Talent("Ronners!!!!!",ronners:50)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!!!"));

            talents.Add(new Talent("Ronners!!!!!!",ronners:60)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!!!!"));

            talents.Add(new Talent("Ronners!!!!!!!",ronners:70)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!!!!"));

            talents.Add(new Talent("Ronners!!!!!!!!",ronners:80)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!!!!!!")
                .AddRequiredTalent("Ronners!!!!!!"));

            talents.Add(new Talent("Ronners!!!!!!!!!",ronners:90)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!!!!!!!"));

            talents.Add(new Talent("Ronners!!!!!!!!!!",ronners:100)
                .AddBonus(BonusType.Happiness,10d)
                .AddBonus(BonusType.Health,10d)
                .AddBonus(BonusType.Hunger,10d)
                .AddRequiredTalent("Ronners!!!!!!!!!"));

            talents.Add(new Talent("Object Permanence",objectivity:1)
                .AddBonus(BonusType.Inventory,0d)
                .AddRequiredTalent("Ronners"));

            talents.Add(new Talent("Self-Sustaining",nutrition:1)
            .AddBonus(BonusType.SelfSustaining,0)
            .AddRequiredTalent("Ronners"));
            return talents;
        }
    }
}