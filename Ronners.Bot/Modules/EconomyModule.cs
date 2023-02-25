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
    
    public class EconomyModule : InteractionModuleBase<SocketInteractionContext>
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


        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("daily","Claim your daily RonPoints.")]
        public async Task dailyAsync()
        {
            var user = Context.User;

            var result = await _economyService.Daily(user);

            if(!result.Success)
            {
                await RespondAsync(result.ErrorMessage);
                return;
            }
                
            
            await RespondAsync(embed:CustomEmbeds.BuildEmbed(user,result));

            var achievementResult = AchievementResult.FromSuccess();

            achievementResult.AchievementType = AchievementType.Daily;
            achievementResult.IntValue = result.Streak;
            achievementResult.User = Context.User;

            AchievementService.ProcessMessage(achievementResult);
        }

        [SlashCommand("give","Give RonPoints to a user.")]
        public async Task giveAsync(IUser user, [MinValue(1)]int amount)
        {
            var giver = Context.User;
            var allowedMentions = new AllowedMentions(null);
            allowedMentions.MentionRepliedUser=true;

            if(amount <0)
            {
                await RespondAsync($"Invalid amount. Amount must be > 0");
                return;
            }
            
            //Remove points from Giver
            if(await GameService.AddRonPoints(giver,-1*amount))
            {
                //Grant points to receiver
                await GameService.AddRonPoints(user,amount);
                await RespondAsync ($"{giver.Mention} gave {amount} RonPoints to {user.Mention}.",allowedMentions:allowedMentions);

                //Log that you gave points to Ronners
                if(user.Id==_discord.CurrentUser.Id)
                {
                    int daysUntilReturn = Rand.Next(1,365);
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    DateTimeOffset returnTime  = now.AddDays(daysUntilReturn);
                    
                    RonGift gift = new RonGift(giver.Id,amount,now.ToUnixTimeSeconds());
                    gift.ReturnDate = returnTime.ToUnixTimeSeconds();
                    gift.ReturnPoints = (int)Math.Floor(amount * Math.Pow(1.015 , (double)daysUntilReturn));
                    await GameService.AddRonnersGift(gift);
                }
                
            }
            else
            {
                await RespondAsync($"You do not have enough points.");
                return;
            }
        }

        [SlashCommand("points","View a user's current RonPoints.")]
        public async Task pointsAsync(IUser user = null)
        {
            user = user ?? Context.User;
            User result;
            int points = 0;
            result = await GameService.GetUserByID(user.Id);
            if(result is not null)
            {
                points = result.RonPoints;
            }
            await RespondAsync($"{user.Username} has {points} RonPoints.");
        }

        [SlashCommand("top", "List the users with the most RonPoints")]
        public async Task topAsync([MinValue(1)] int count=10)
        {
            var users = await GameService.GetUsers();
            var response = "";
            foreach(var user in users.OrderByDescending(p => p.RonPoints).Take(count))
            {

                if(response.Length + user.PointString().Length > 2000)
                {
                    await RespondAsync(response);
                    response = "";
                }
                response += user.PointString()+"\n";
           
            }
            await RespondAsync(response);
        }
    }
}