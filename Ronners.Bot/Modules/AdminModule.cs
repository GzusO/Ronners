using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Ronners.Bot.Services;


namespace Ronners.Bot.Modules
{
    [Discord.Commands.Group("admin")]
    [Discord.Commands.RequireOwner]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        public CommandService _commandService {get;set;}
        public InteractionService _interactionService {get;set;}


        [Command("help")]
        [Alias("?")]
        [Discord.Commands.Summary("USAGE: !admin help {PAGE:INT}")]
        public async Task Help(int page = 1)
        {
            if(page < 1)
                page = 1;
            var skip = 25*(page-1);
            var module = _commandService.Modules.First(mod => mod.Name=="admin");
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
        [Command("scadd")]
        [Discord.Commands.Summary("Adds Slash Commands to the Server. USAGE: !admin scadd")]
        public async Task SlashCommandAddAsync()
        {
            await _interactionService.RegisterCommandsToGuildAsync(Context.Guild.Id);
        }
    }
}