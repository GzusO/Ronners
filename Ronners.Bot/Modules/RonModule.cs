using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Models;
using Ronners.Bot.Services;
using Ronners.Bot.Extensions;


namespace Ronners.Bot.Modules
{
    [Group("ron")]
    public class RonModule : ModuleBase<SocketCommandContext>
    {
        public GameService GameService{ get; set; }
        public CommandService _commandService {get;set;}
        public RonService RonService{get;set;}

        [Command("help")]
        [Alias("?")]
        [Summary("USAGE: !ron help {PAGE:INT}")]
        public async Task Help(int page = 1)
        {
            if(page < 1)
                page = 1;
            var skip = 25*(page-1);
            var module = _commandService.Modules.First(mod => mod.Name=="ron");
            var commands = module.Commands;
            EmbedBuilder embedBuilder = new EmbedBuilder();
            

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";
                embedBuilder.AddField($"{module.Group} {command.Name}", embedFieldText);
            }

            var commandCount = module.Commands.Count();
            int pageCount = (commandCount + 24)/ 25;

            await ReplyAsync($"Commands Page [{page}/{pageCount}]: ", false, embedBuilder.Build());
        }

        [Command("feed")]
        [Summary("Feed Ronners, costs 10 RonPoints provides ~10 hunger.\nUSAGE: !ron feed")]
        public async Task FeedAsync()
        {
            if(!await GameService.AddRonPoints(Context.User,-10))
            {
                await ReplyAsync("Not enough points! Costs 10 RonPoints.");
                return;
            }
            var food = await RonService.FeedRon();
            await ReplyAsync($"You fed Ronners {food}.");
        }

        [Command("play")]
        [Summary("Play with Ronners, costs 10 RonPoints.\nUSAGE: !ron play")]
        public async Task PlayAsync()
        {
            if(!await GameService.AddRonPoints(Context.User,-10))
            {
                await ReplyAsync("Not enough points! Costs 10 RonPoints.");
                return;
            }
            var game = await RonService.PlayWithRon();
            await ReplyAsync($"You played {game} with Ronners.");
        }
        
        [Command("heal")]
        [Summary("Heal Ronners, Cost 200 RonPoints.\nUSAGE: !ron heal")]
        public async Task HealAsync()
        {
            if(!await GameService.AddRonPoints(Context.User,-200))
            {
                await ReplyAsync("Not enough points! Costs 200 RonPoints.");
                return;
            }
            await RonService.HealRon();
            await ReplyAsync($"You healed Ronners.");
        }

        [Command("level")]
        [Summary("Levels up Ronners.\nUSAGE !ron level")]
        public async Task LevelAsync()
        {
            var cost = -100;

            if(!await GameService.AddRonPoints(Context.User,cost))
            {
                await ReplyAsync("Not enough points! Cost 100 RonPoints.");
                return;
            }
            var level = await RonService.LevelUp();
            if(level < 0)
            {
                await GameService.AddRonPoints(Context.User,-1*cost);//Refund RonPoints
                await ReplyAsync($"Not enough Experience! Need {level*-1} experience.");
                return;
            }
            await ReplyAsync($"Ronners leveled up to level: {level}");
        }
        
        [Command("info")]
        [Summary("Display Ronners Info.\nUSAGE: !ron info")]
        public async Task InfoAsync()
        {
            var embed = RonService.GetStateEmbed();
            await ReplyAsync(null,false,embed);
        }

        [Command("skillup")]
        [Summary("Increase skill.\nUSAGE: !ron skillup ronners")]
        public async Task SkillUp(Skill skill)
        {
            var cost = -100;

            if(!await GameService.AddRonPoints(Context.User,cost))
            {
                await ReplyAsync("Not enough points! Cost 100 RonPoints.");
                return;
            }
            var level = await RonService.SkillUp(skill);
            if(level==0)
            {
                await GameService.AddRonPoints(Context.User,-1*cost);//Refund RonPoints
                await ReplyAsync("No skill points available.");
                return;
            }
            await ReplyAsync($"{skill.GetEnumDescription()} -> {level}.");
        }

        

        [Command("professions")]
        [Summary("List Ronners Professions.\nUSAGE: !ron professions")]
        public async Task ProfessionsAsync()
        {
            var embed = RonService.GetProfessionEmbed();
            await ReplyAsync(null,false,embed);
        }

        [Command("subscribe")]
        [RequireOwner]
        [Summary("Subscribes Ronners to channel, creating a Thread for RonPG")]
        public async Task SubscribeAsync(ITextChannel channel = null)
        {
            channel = channel ?? (Context.Channel as ITextChannel);
            if(RonService.GuildAlreadyHasThread(Context.Guild.Id))
            {
                await ReplyAsync("Server already has a RonPG Events Thread");
                return;
            }
            var thread = await channel.CreateThreadAsync("RonPG Events");
            await RonService.AddThread(thread.Id,thread.GuildId);
        }

        [Group("talent")]
        public class TalentModule : ModuleBase<SocketCommandContext>
        {
            public GameService GameService{ get; set; }
            public CommandService _commandService {get;set;}
            public RonService RonService{get;set;}

            [Command("list")]
            [Summary("Lists talents")]
            public async Task DefaultTalentAsync(int page = 1)
            {
                if(page<1)
                    page = 1;
                var embed = RonService.GetTalentListEmbed(page);
                await ReplyAsync("",false,embed);
            }
            [Command("info")]
            [Summary("Get info about specified talent")]
            public async Task TalentAsync([Remainder]string talent)
            {
                if(!RonService.TalentExists(talent))
                {
                    await ReplyAsync($"Unknown talent: {talent}");
                    return;
                }
                    
                var embed = RonService.GetTalentEmbed(talent);
                await ReplyAsync("",false,embed);
            }
            [Command("help")]
            [Alias("?")]
            [Summary("USAGE: !ron talent help {PAGE:INT}")]
            public async Task Help(int page = 1)
            {
                if(page < 1)
                    page = 1;
                var skip = 25*(page-1);
                var module = _commandService.Modules.First(mod => mod.Name=="talent");
                var commands = module.Commands;
                EmbedBuilder embedBuilder = new EmbedBuilder();
                

                foreach (CommandInfo command in commands)
                {
                    // Get the command Summary attribute information
                    string embedFieldText = command.Summary ?? "No description available\n";
                    embedBuilder.AddField($"{module.Group} {command.Name}", embedFieldText);
                }

                var commandCount = module.Commands.Count();
                int pageCount = (commandCount + 24)/ 25;

                await ReplyAsync($"Commands Page [{page}/{pageCount}]: ", false, embedBuilder.Build());
            }

            [Command("add")]
            [Summary("Add a talent, costs 100 RonPoints.\nUSAGE: !ron talent add Ronners!!!")]
            public async Task AddTalentAsync([Remainder]string talent)
            {
                var cost = -100;
                if(!RonService.TalentExists(talent))
                {
                    await ReplyAsync($"No talent: {talent}");
                    return;
                }
                if(!RonService.CanAfford(talent))
                {
                    await ReplyAsync($"Not enough Talent Points.");
                    return;
                }
                var prereqs = "";
                if(!RonService.CanAcquire(talent,out prereqs))
                {
                    await ReplyAsync($"Missing Required Talents: {prereqs}");
                    return;
                }

                if(!await GameService.AddRonPoints(Context.User,cost))
                {
                    await ReplyAsync($"Can't afford. Costs{-1*cost} RonPoints");
                    return;
                }
                await RonService.AddTalent(talent);
            }


            [Command("grantpoint")]
            [Alias("granttp")]
            [RequireOwner]
            [Summary("Admin command to add talent points.\nUSAGE: !ron grantpoint 10")]
            public async Task GrantTalentPointAsnyc(int amount=1)
            {
                await RonService.AddTalentPoints(amount);
            }
        }
    }
}