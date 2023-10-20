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
    public class BlackjackService
    {

        public Dictionary<string, BlackjackState> Instances{get;set;} = new Dictionary<string, BlackjackState>();
        public Random _rand{get;set;}

        internal BlackjackState GetGame(string id)
        {
            BlackjackState state;
            if(Instances.TryGetValue(id, out state))
                return state;
            return null;
        }

        internal BlackjackState StartGame(ulong id)
        {
            string guid = $"{new Guid()}{id}";
            if(Instances.ContainsKey(guid))
                return null;
            var bjs = new BlackjackState(_rand,guid);
            
            if(bjs.GameState == Outcome.InProgress)
                Instances.Add(guid,bjs);

            return bjs;
        }

        internal BlackjackState HitGame(string id)
        {
            var game = GetGame(id);
            if(game == null)
                return null;
            
            game.Hit();
            if(game.GameState != Outcome.InProgress)
                Instances.Remove(id);
            return game;
        }
        
        internal BlackjackState StayGame(string id)
        {
            var game = GetGame(id);
            if(game == null)
                return null;
            game.Stay();
            if(game.GameState != Outcome.InProgress)
                Instances.Remove(id);
            return game;
        }
    }

    public class BlackjackState
    {
        public Deck CardDeck{get;set;}
        public List<Card> Player{get;set;} = new List<Card>();
        public List<Card> Dealer{get;set;} = new List<Card>();
        public Outcome GameState {get;set;} = Outcome.Starting;
        public bool PlayerTurn {get;set;}= false;
        public bool CanHit {get{return GameState==Outcome.InProgress && PlayerTurn;}}
        public bool CanStay {get{return GameState==Outcome.InProgress && PlayerTurn;}}
        public string GameID {get;set;}

        public BlackjackState(Random rand,string id)
        {
            GameID = id;
            CardDeck = new Deck(51,rand,1,0);
            CardDeck.Shuffle();
            Player.AddRange(CardDeck.Draw(1));
            Dealer.AddRange(CardDeck.Draw(1));
            Player.AddRange(CardDeck.Draw(1));
            Dealer.AddRange(CardDeck.Draw(1));
            
            CheckHandsForBlackjack();

            if(GameState == Outcome.InProgress)
                PlayerTurn = true;
        }

        public void Hit()
        {
            Player.AddRange(CardDeck.Draw(1));
            
            CheckForBust();
            if(GameState == Outcome.Lose)
                PlayerTurn = false;
        }

        internal void Stay()
        {
            //Advance Game state
            DealerTurn();
            CalculateWinner();
        }


        //Game will be over after this and GameState will appropriately be set
        private void DealerTurn()
        {
            var dealerValue = CalculateHandValue(Dealer);

            while(dealerValue <= 16)
            {
                Dealer.AddRange(CardDeck.Draw(1));
                dealerValue = CalculateHandValue(Dealer);
            }
        }

        private void CalculateWinner()
        {
            var playerValue = CalculateHandValue(Player);
            var dealerValue = CalculateHandValue(Dealer);
            if(playerValue > dealerValue)
                GameState = Outcome.Win;
            else if(playerValue < dealerValue && dealerValue <= 21) //Lose if dealer didn't bust
                GameState = Outcome.Lose;
            else 
                GameState = Outcome.Draw;
        }

        private void CheckForBust()
        {
            var playerValue = CalculateHandValue(Player);
            if(playerValue > 21)
                GameState = Outcome.Lose;
        }

        private void CheckHandsForBlackjack()
        {
            var playerValue = CalculateHandValue(Player);
            var dealerValue = CalculateHandValue(Dealer);
            
            if(playerValue == 21 && playerValue!= dealerValue) //Player Blackjack
                GameState = Outcome.Blackjack;
            else if(playerValue == 21 && playerValue == dealerValue) //Dealer + Player Blackjack
                GameState = Outcome.Draw;
            else if(dealerValue == 21 && playerValue != dealerValue) //Dealer Blackjack
                GameState = Outcome.Lose;
            else
                GameState = Outcome.InProgress;
        }

        private int CalculateHandValue(IEnumerable<Card> hand)
        {
            var sum = 0;
            bool hasAce = false;
            foreach (var card in hand)
            {
                if(card.number==1)
                    hasAce = true;
                sum+= card.GetBlackJackCardValue();
            }
            //make Ace low
            if(sum > 21 && hasAce)
                sum-=10;
            return sum;
        }
    }

    public enum Outcome
    {
        Starting,
        InProgress,
        Lose,
        Draw,
        Win,
        Blackjack
    }
}