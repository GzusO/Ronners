// Interation modules must be public and inherit from an IInterationModuleBase
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Ronners.Bot.Models;
using Ronners.Bot.Services;

namespace Ronners.Bot.Modules
{
    [Group("rpg","Role Playing Game")]
    public class RPGModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Interactions { get; set; }
        public WebService _webService{get;set;}
        public ImageService _imageService{get;set;}
        public GameService GameService{get;set;}
        public AchievementService AchievementService{get;set;}
        public Random Rand{get;set;}
        public DiscordSocketClient _discord{get;set;}

        public EconomyService _economyService{get;set;}
        public BattleService BattleService {get;set;}


        [SlashCommand("stats","Lookup stats for a character")]
        public async Task StatsAsync(IUser user = null)
        {
            user ??= Context.Interaction.User;
            string mention = MentionUtils.MentionUser(Context.Interaction.User.Id);

            var allowedMentions = new AllowedMentions(null);

            Embed characterDetails = BattleService.GetCharacterDetails(user.Id);

            await RespondAsync(mention,null,false,false,allowedMentions,null,null,characterDetails);
        }


        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("create","Create a character.")]
        public async Task CreateAsync()
        {
            var hasCharacter = BattleService.CharacterExists(Context.User.Id);

            if(hasCharacter)
            {
                await RespondAsync("You already have a character.",ephemeral: true);
                return;
            }

            await RespondWithModalAsync<CreateCharacterModal>("create_character");
        }

        [ModalInteraction("create_character",true)]
        public async Task CreateCharacterModalAsync(CreateCharacterModal modal)
        {
            BattleService.CreateCharacter(Context.Interaction.User.Id, modal.CharacterName);
            await RespondAsync("Success",ephemeral:true);
        }


    }

    public class CreateCharacterModal : IModal
    {
        public string Title => "Character Creation";

        [RequiredInput(true)]
        [InputLabel("Name")]
        [ModalTextInput("character_name",TextInputStyle.Short, placeholder:"Enter character name")]
        public string CharacterName{get;set;}
    }
}