using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Ronners.Bot;
using Ronners.Bot.Models;

namespace Ronners.Test
{
    [TestClass]
    public class DeckTest
    {
        [TestMethod]
        public void TestDraw52()
        {
            var Deck = new Deck(51,new Random(0),1,0);
            var cards = Deck.Draw(52);
            var fullDeck = GetFullDeck();
            
            Debug.WriteLine($"Drawn Deck contains {cards.Count} cards.");
            Assert.IsTrue(cards.Count == 52,"Failed to Draw 52 Cards from 52 card Deck");
            
            bool AllCardsInDeck = true;

            foreach (var card in fullDeck)
            {
                if(!cards.Any(x=> x.number == card.number && x.suit == card.suit))
                {
                    Debug.WriteLine($"Deck is missing card {card}");
                    AllCardsInDeck = false;
                    break;
                }
            }
            Assert.IsTrue(AllCardsInDeck,"Deck Doesn't Contain all 52 cards when it should.");
        }

        public List<Card> GetFullDeck()
        {
            var deck = new List<Card>();
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(Suit.Club,i));
                deck.Add(new Card(Suit.Diamond,i));
                deck.Add(new Card(Suit.Heart,i));
                deck.Add(new Card(Suit.Spade,i));
            }
            return deck;
        }

    }
}