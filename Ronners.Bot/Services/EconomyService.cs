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
    public class EconomyService
    {
        
        private readonly Random _rand;
        private readonly GameService _gameService;
        public EconomyService(Random rand, GameService gs)
        {
            _rand = rand; 
            _gameService = gs;
        }


        public async Task<DailyResult> Daily(IUser user)
        {
            return await Daily(user,false);
        }
        private async Task<DailyResult> Daily(IUser user, bool testing = false)
        {
            DailyResult result = new DailyResult(_rand);

            UserDaily daily = await _gameService.GetUserDaily(user);
            int completedCollections = await _gameService.GetUserCompletedCollectionCount(user);

            if (daily is null)
            {
                daily = new UserDaily(){UserID=user.Id,LastCheckIn = 0,Streak =0};
                await _gameService.AddUserDaily(daily);
            }

            int daysSinceLastCheckIn = (DateTime.UtcNow.Date - DateTimeOffset.FromUnixTimeSeconds(daily.LastCheckIn).Date).Days;
            daily.LastCheckIn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            //Same Day
            if (daysSinceLastCheckIn < 1 && !testing)
            {
                DateTime now = DateTime.UtcNow;
                DateTime nextDay = now.Date.AddDays(1);
                TimeSpan timeToNewDay = nextDay-now;
                result.Success = false;
                result.ErrorMessage = $"Already claimed Daily. Please wait {timeToNewDay.ToString("hh\\:mm\\:ss")}";
            }
            else
            {
                if(daysSinceLastCheckIn == 1)
                    daily.Streak++;
                else
                    daily.Streak = 1;
                

                result.CalculateDaily((await _gameService.GetUserByID(user.Id)).RonPoints,daily.Streak,completedCollections);

                if(!testing)
                {
                    await _gameService.AddRonPoints(user,result.TotalBonus);
                    await _gameService.UpdateUserDaily(daily);
                }
            }
            
            return result;
        }

        public async Task<DailyResult> TestDaily(IUser user)
        {
            return await Daily(user,true);
        }
    }
}