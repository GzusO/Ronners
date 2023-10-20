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
        private BattleManager battleManager;
        public BattleService(Random rand)
                    => _rand = rand;


        public async Task InitializeAsync()
        {
            battleManager = new BattleManager("players.json","monsters.json","weapons.json");
            battleManager.LoadData();
        }

        public BattleResult Demo(string name, int ronners, int objectivity, int normalcy, int nutrition, int erudition, int rapidity, int strength, string weapon)
        {
            if (rapidity == 0)
                rapidity = _rand.Next(1,4);
                
            Combatant player = new Combatant(ronners,objectivity,normalcy,nutrition,erudition,rapidity,strength).SetName(name);
            Combatant enemy = new Combatant(rapidity:_rand.Next(1,3), strength:2).SetName("Rat");

            player.HandSlot = CombatHelpers.GetRandomWeapon(new RandomGenerator(_rand), weapon);

            return new BattleResult(CombatHelpers.Battle(new RandomGenerator(_rand),player,enemy),player,enemy);
        }


        public bool CharacterExists(ulong id)
        {
            return battleManager.Players.Any(x=> x.UserID == id);
        }

        public void CreateCharacter(ulong id, string name)
        {
            battleManager.AddPlayer(id,name);
        }

        internal Embed GetCharacterDetails(ulong id)
        {
            Combatant player = battleManager.GetPlayerByID(id);
            var builder = new EmbedBuilder();
            if(player is null)
            {
                builder.WithTitle("No character exists.")
                .WithColor(Color.Red)
                .WithCurrentTimestamp();
                return builder.Build();
            }
            else
            {
                builder.WithTitle($"{player.Name}'s Stats  ({player.Experience} xp)");

                //Add Characterisitics
                builder.AddField("Ronners",$"`{player.Ronners}`",true);
                builder.AddField("Objectivity",$"`{player.Objectivity}`",true);
                builder.AddField("Normalcy",$"`{player.Normalcy}`",true);
                builder.AddField("Nutrition",$"`{player.Nutrition}`",true);
                builder.AddField("Erudition",$"`{player.Erudition}`",true);
                builder.AddField("Rapidity",$"`{player.Rapidity}`",true);
                builder.AddField("Strength",$"`{player.Strength}`",true);

                builder.AddField("HP",$"`{player.CurrentHealth} / {player.MaxHealth}`",false);

                //Misc
                builder.AddField("Attack Speed", $"`{player.AttackSpeed}`",true);
                builder.AddField("Critical Chance", $"`{player.CriticalHitChance}%`",true);
                builder.AddField("Critical Damage", $"`{player.CriticalHitMultiplier}x`",true);
                builder.AddField("Damage",$"`{player.MinDamage} - {player.MaxDamage}`",true);

                //Gear
                var mainHandName = player.HandSlot is null ? "nothing" : player.HandSlot.Name;
                builder.AddField("Main Hand",$"{mainHandName}");


                return builder.Build();
            }
        }
    } 
}