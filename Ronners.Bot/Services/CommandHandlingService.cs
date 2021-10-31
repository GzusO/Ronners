using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Ronners.Bot.Models;

namespace Ronners.Bot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        private readonly AchievementService _achievements;
        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _achievements = services.GetRequiredService<AchievementService>();
            
            _services = services;
            // Hook CommandExecuted to handle post-command-execution logic.
            _commands.CommandExecuted += CommandExecutedAsync;
            // Hook MessageReceived so we can process each message to see
            // if it qualifies as a command.
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            //await _services.GetRequiredService<GameService>().AddRonPoint(rawMessage.Author);

            // This value holds the offset where the prefix ends
            var argPos = 0;
            // Perform prefix check. You may want to replace this with
             
            // for a more traditional command format like !help.
            if (!message.HasCharPrefix(ConfigService.Config.CommandPrefix, ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);



            // Perform the execution of the command. In this method,
            // the command service will perform precondition and parsing check
            // then execute the command if one is matched.
            await _commands.ExecuteAsync(context, argPos, _services); 
            // Note that normally a result will be returned by this format, but here
            // we will handle the result in CommandExecutedAsync,
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            switch(result)
            {
                case AchievementResult customResult:
                    if(!customResult.IsSuccess)
                        await context.Channel.SendMessageAsync($"Error: {result}");
                    else if(customResult.IsSuccess)
                    {
                        _achievements.ProcessMessage(customResult);
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(result.ErrorReason))
                        await context.Channel.SendMessageAsync($"Error: {result}");
                    break;
            }
        }
    }
}