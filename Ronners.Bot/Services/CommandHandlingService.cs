using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Interactions;
using Discord.Commands;
using Discord.WebSocket;
using Ronners.Bot.Models;
using System.Linq;

namespace Ronners.Bot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        private readonly AchievementService _achievements;

        private readonly Random _rand;
        private readonly InteractionService _interactions;
        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _achievements = services.GetRequiredService<AchievementService>();
            _rand = services.GetRequiredService<Random>();
            _interactions = services.GetRequiredService<InteractionService>();
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

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(),_services);

            _discord.InteractionCreated += HandleInteraction;

            _interactions.SlashCommandExecuted += SlashCommandExecuted;
            _interactions.ContextCommandExecuted += ContextCommandExecuted;
            _interactions.ComponentCommandExecuted += ComponentCommandExecuted;
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            if(message.Author.Id == 146979125675032576)//Block squirtle
                return;

            if(_rand.Next(0,10) == 0)
            {
                SocketGuild guild = ((SocketGuildChannel)rawMessage.Channel).Guild;
                IEmote emote = guild.Emotes.FirstOrDefault(e => e.Name == "ronners");
                if(emote is not null)
                    await rawMessage.AddReactionAsync(emote);

                await _services.GetRequiredService<GameService>().AddRonPoints(rawMessage.Author,10);
            } 
           

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

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
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

         private Task ComponentCommandExecuted (ComponentCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }    

            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted (ContextCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted (SlashCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private async Task HandleInteraction (SocketInteraction arg)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_discord, arg);
                await _interactions.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if(arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}