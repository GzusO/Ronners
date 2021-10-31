using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using Ronners.Bot.Extensions;
using System.Linq;
using Ronners.Bot.Models;

namespace Ronners.Bot.Services
{
    public class AchievementService
    {
        
        private readonly Random _rand;
        private readonly GameService _gameService;
        private readonly DiscordSocketClient _discord;
        public AchievementService(Random rand, GameService gs, DiscordSocketClient dsc)
        {
            _rand = rand; 
            _gameService = gs;
            _discord = dsc;
        }

        public void ProcessMessage(AchievementResult result)
        {
            AddAchievementMessage(result);

            if(result.User.Id == 312458335574294530)
                GrantAchievement(4,result.User.Id);

            switch(result.AchievementType)
            {
                case AchievementType.Roll:
                    CheckRollAchievements(result);
                    break;
                case AchievementType.EightBall:
                    CheckEightBallAchievements(result);
                    break;
                case AchievementType.Ideas:
                    CheckIdeaAchievements(result);
                    break;
                case AchievementType.Captcha:
                    CheckCaptchaAchievements(result);
                    break;
                case AchievementType.Slurp:
                    CheckSlurpAchievements(result);
                    break;
                default:
                    break;
            }
        }

        private async void CheckSlurpAchievements(AchievementResult result)
        {
            var messages = await _gameService.GetAchievementMessagesByTypeAndUserId((int)AchievementType.Slurp,result.User.Id);
            if(messages.Count() > 100 )
                GrantAchievement(9,result.User.Id);
        }

        private async void CheckCaptchaAchievements(AchievementResult result)
        {
            var messages = await _gameService.GetAchievementMessagesByTypeAndUserId((int)AchievementType.Captcha,result.User.Id);

            if(messages.Where(x => x.BoolValue.HasValue && x.BoolValue.Value == true).Count() >= 100)
            {
                GrantAchievement(7,result.User.Id);
            }
            if(messages.Where(x => x.BoolValue.HasValue && x.BoolValue.Value == false).Count() >= 100)
            {
                GrantAchievement(8,result.User.Id);
            }
        }

        private async void CheckIdeaAchievements(AchievementResult result)
        {
            var messages = await _gameService.GetAchievementMessagesByTypeAndUserId((int)result.AchievementType,result.User.Id);

            if(messages.Count() >= 10)
                GrantAchievement(5,result.User.Id);
        }

        private async void CheckEightBallAchievements(AchievementResult result)
        {
            var messages = await _gameService.GetAchievementMessagesByTypeAndUserId((int)result.AchievementType,result.User.Id);

            //Check if the three most recent 8balls were yes results.
            var latestThree = messages.OrderByDescending(x => x.AchievementMessageId).Take(3);
            if(latestThree.Count() == 3 && latestThree.All(y => y.IntValue == 1))
                GrantAchievement(2, result.User.Id);
        }

        private async void AddAchievementMessage(AchievementResult result)
        {
            var msg = new AchievementMessage(result);
            await _gameService.InsertAchievementMessage(msg);
        }

        private void CheckRollAchievements(AchievementResult result)
        {
            if(result.IntValue == 69)
                GrantAchievement(1,result.User.Id);
            if(result.IntValue == 420)
                GrantAchievement(6,result.User.Id);
        }

        private async void GrantAchievement(ulong achievementId, ulong userid)
        {
            if(await _gameService.HasAchievement(achievementId, userid))
                return;
            await _gameService.InsertUserAchievement(achievementId, userid);
            NotifyAchievementGet(userid, achievementId);
        }

        private async void NotifyAchievementGet(ulong userid, ulong achievementId)
        {
            var user = _discord.GetUser(userid);
            Achievement achievement = await _gameService.GetAchievement(achievementId);

            await user.SendMessageAsync($"You have achieved {achievement}! Ronners!");
        }
    }
}