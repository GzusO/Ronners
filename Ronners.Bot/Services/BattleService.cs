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
using Ronners.RPG;

namespace Ronners.Bot.Services
{
    public class BattleService
    {
        private readonly Random _rand;
        public BattleService(Random rand)
                    => _rand = rand;

        public BattleResult Demo(string name, int ronners, int objectivity, int normalcy, int nutrition, int erudition, int rapidity, int strength, string weapon)
        {
            if (rapidity == 0)
                rapidity = _rand.Next(1,4);
                
            Combatant player = new Combatant(ronners,objectivity,normalcy,nutrition,erudition,rapidity,strength).SetName(name);
            Combatant enemy = new Combatant(rapidity:_rand.Next(1,3), strength:2).SetName("Rat");

            player.HandSlot = CombatHelpers.GetRandomWeapon(new RandomGenerator(_rand), weapon);

            return new BattleResult(CombatHelpers.Battle(new RandomGenerator(_rand),player,enemy),player,enemy);
        }
    } 
}