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
    public class BaccaratService
    {
        private Deck _deck;

        public BaccaratService(Random rand)
        {
            _deck = new Deck(45,rand,1,0);
            _deck.Shuffle();
        }
    
        public (int,string) Play(BaccaratBetType betType, int amount)
        {  
            bool playerDrew = false;

            List<Card> playerHand = _deck.Draw(2);
            List<Card> bankerHand = _deck.Draw(2);

            Card bankerThird;
            Card playerThird =null;
            int playerVal = GetHandValue(playerHand);
            int bankerVal = GetHandValue(bankerHand);

            if(playerVal == 8 || playerVal ==9 || bankerVal == 8 || bankerVal == 9)
            {
                
            }
            else 
            {
                if(playerVal >= 0 && playerVal <= 5)
                {
                    playerDrew =true;
                    playerThird = _deck.Draw();
                    playerHand.Add(playerThird);
                }

                if(playerDrew)
                {
                    if(bankerVal <= 2)
                    {
                        bankerThird = _deck.Draw();
                        bankerHand.Add(bankerThird);
                    }
                    else if(bankerVal ==3)
                    {
                        if(playerThird.number !=8)
                        {
                            bankerThird = _deck.Draw();
                            bankerHand.Add(bankerThird);
                        }
                    }
                    else if(bankerVal == 4)
                    {
                        if(playerThird.number == 2 || playerThird.number == 3 || playerThird.number == 4 || playerThird.number == 5 || playerThird.number == 6 || playerThird.number == 7 )
                        {
                            bankerThird = _deck.Draw();
                            bankerHand.Add(bankerThird);
                        }
                    }
                    else if(bankerVal ==5)
                    {
                        if(playerThird.number == 4 || playerThird.number == 5 || playerThird.number == 6 || playerThird.number == 7 )
                        {
                            bankerThird = _deck.Draw();
                            bankerHand.Add(bankerThird);
                        }
                    }
                    else if(bankerVal == 6)
                    {
                        if(playerThird.number ==6 || playerThird.number ==7)
                        {
                            bankerThird = _deck.Draw();
                            bankerHand.Add(bankerThird);
                        }
                    }
                }
                else if(bankerVal >= 0 && bankerVal <=5)
                {
                    bankerThird = _deck.Draw();
                    bankerHand.Add(bankerThird);
                }
            }

            playerVal = GetHandValue(playerHand);
            bankerVal = GetHandValue(bankerHand);

            if(playerVal > bankerVal)
                if(betType == BaccaratBetType.Player)
                    return(amount+amount,$"Player Wins!\n{GetResultString(playerHand,bankerHand)}");
                else
                    return(0,$"Player Wins!\n{GetResultString(playerHand,bankerHand)}");
            else if( playerVal < bankerVal)
                if(betType == BaccaratBetType.Banker)
                    return ((int)(amount*.95)+amount,$"Banker Wins!\n{GetResultString(playerHand,bankerHand)}");
                else
                    return(0,$"Banker Wins!\n{GetResultString(playerHand,bankerHand)}");
            else
                if(betType == BaccaratBetType.Tie)
                    return(8*amount+amount,$"Tie!\n{GetResultString(playerHand,bankerHand)}");
                else
                    return(0,$"Tie!\n{GetResultString(playerHand,bankerHand)}");
        }

        private string GetResultString(List<Card> playerHand, List<Card> bankerHand)
        {
            string playerCards = "";
            string bankerCards = "";
            foreach(var card in playerHand)
                playerCards+=card.ToString()+",";
            foreach(var card in bankerHand)
                bankerCards+= card.ToString()+",";
            playerCards.Trim(',');
            bankerCards.Trim(',');
            return $"Player Hand: {playerCards}\nBanker Hand: {bankerCards}\n";
        }

        private int GetHandValue(List<Card> hand)
        {
            int val =0;
            foreach(var card in hand)
            {
                switch(card.number)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        val += card.number;
                        break;
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        val += 0;
                        break;
                }
            }
            //Max value in Baccarat is 9 if higher remove subtract 10;
            while(val > 9)
            {
                val -= 10;
            }
            return val;
        }

        public BaccaratBetType GetBetType(string bet)
        {
            switch(bet.ToLower())
            {
                case "player":
                case "p":
                    return BaccaratBetType.Player;
                case "banker":
                case "b":
                    return BaccaratBetType.Banker;
                case "tie":
                case "t":
                    return BaccaratBetType.Tie;
                default:
                    return BaccaratBetType.Unknown;
            }
        }
    }
    public enum BaccaratBetType
    {
        Player,
        Banker,
        Tie,
        Unknown
    }  
}