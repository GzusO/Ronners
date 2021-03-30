using System;
using System.Collections.Generic;
using Ronners.Bot.Extensions;

namespace Ronners.Bot.Models
{
    public class Deck
    {
        private List<Card> _deck;
        private int _deckIndex;
        private int _shufflePoint;
        private Random _rand;

        public Deck (int shufflePoint, Random rand =null, int deckCount = 1, int jokerCount = 0)
        {
            _rand = rand ?? new Random();
            _shufflePoint = shufflePoint;
            _deck = new List<Card>();
            _deckIndex =0;

            while(deckCount>=1)
            {
                for (int i = 1; i <= 13; i++)
                {
                    _deck.Add(new Card(Suit.Club,i));
                    _deck.Add(new Card(Suit.Diamond,i));
                    _deck.Add(new Card(Suit.Heart,i));
                    _deck.Add(new Card(Suit.Spade,i));
                }
                for(int i =0;i<jokerCount;i++)
                {
                    _deck.Add(new Card(Suit.Joker,0));
                }
                deckCount--;
            }
        }

        public void Shuffle()
        {
            _deckIndex = 0;
            _deck.Shuffle<Card>(_rand);
        }
        public List<Card> Draw(int count)
        {
            ShuffleIfNeeded();

            var drawnCards = _deck.GetRange(_deckIndex,count);
            _deckIndex+=count;

            return drawnCards;
        }

        public Card Draw()
        {
            ShuffleIfNeeded();
            var drawnCard = _deck.GetRange(_deckIndex,1)[0];
            _deckIndex+=1;

            return drawnCard;
        }
        
        private void ShuffleIfNeeded()
        {
            if(_deckIndex >= _shufflePoint)
                Shuffle();
        }
    }
    public class Card
    {
        public Suit suit{get;set;}
        public int number {get;set;}

        public Card(Suit s, int n)
        {
            suit=s;
            number = n;
        }

        public override string ToString()
        {
            string suitString = "";
            string valString = "";
            switch(suit)
            {
                case Suit.Club:
                    suitString = "\u2667";
                    break;
                case Suit.Diamond:
                    suitString = "\u2662";
                    break;
                case Suit.Spade:
                    suitString = "\u2664";
                    break;
                case Suit.Heart:
                    suitString = "\u2661";
                    break;
                case Suit.Joker:
                    suitString = "Joker";
                    break;
            }
            switch(number)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8: 
                case 9:
                case 10:
                    valString = number.ToString();
                    break;
                case 0:
                    valString = "";
                    break;
                case 1:
                    valString = "A";
                    break;
                case 11:
                    valString = "J";
                    break;
                case 12:
                    valString = "Q";
                    break;
                case 13:
                    valString = "K";
                    break;
            }
            return  $" {valString}{suitString}";
        }
    }
    public enum Suit
    {
        Club,
        Spade,
        Heart,
        Diamond,
        Joker
    }

}