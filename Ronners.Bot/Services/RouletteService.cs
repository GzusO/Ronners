using System.Collections.Generic;
using System;
using Ronners.Bot.Extensions;
using System.Linq;
namespace Ronners.Bot.Services
{
    public enum BetType
    {
        Straight,
        Split,
        Street,
        Corner,
        Trio,
        Basket,
        Low,
        High,
        Red,
        Black,
        Even,
        Odd,
        Dozen,
        Column,
        Snake,
        Unknown
    }

    public class Bet
    {
        public BetType Type{get;set;}
        public List<string> Bets{get;set;}

        public Bet(BetType type, string [] bets)
        {
            Bets = new List<string>();
            Type = type;

            Bets.AddRange(bets);

            string s;
            switch(type)
            {
                case BetType.Basket:
                    Bets = new List<string>{"0", "00", "1", "2", "3"};
                    break;
                case BetType.Snake:
                    Bets = new List<string>{"1", "5", "9", "12", "14", "16", "19", "23", "27", "30", "32", "34"};
                    break;
                case BetType.High:
                    Bets = new List<string>{"19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30","31","32","33","34","35","36"};
                    break;
                case BetType.Low:
                    Bets = new List<string>{"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12","13","14","15","16","17","18"};
                    break;
                case BetType.Dozen:
                    s = Bets[0];
                    if(s =="1")
                        Bets = new List<string>{"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"};
                    else if(s =="2")
                        Bets = new List<string>{"13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24"};
                    else
                        Bets = new List<string>{"25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36"};
                    break;
                case BetType.Column:
                    s = Bets[0];
                    if(s =="1")
                        Bets = new List<string>{"1", "4", "7", "10", "13", "16", "19", "22", "25", "28", "31", "34"};
                    else if(s =="2")
                        Bets = new List<string>{"2", "5", "8", "11", "14", "17", "20", "23", "26", "29", "32", "35"};
                    else
                        Bets = new List<string>{"3", "6", "9", "12", "15", "18", "21", "24", "27", "30", "33", "36"};
                    break;
            }
        }
    }
    public class RouletteService
    {
        private readonly List<string> Slots = new List<string>{"0","28","9","26","30","11","7","20","32","17","5","22","34","15","3","24","36","13","1","00","27","10","25","29","12","8","19","31","18","6","21","33","16","4","23","35","14","2"};

        public (int,string) GetResult(Bet bet, int amount)
        {
            var rand = new Random();
            string result = Slots[rand.Next(Slots.Count)];
            
            switch(bet.Type)
            {
                case BetType.Straight:
                    if(bet.Bets.Exists(x=> x==result))
                        return (35*amount + amount,result);
                    break;
                case BetType.Split:
                    if(bet.Bets.Exists(x=> x==result))
                        return (17*amount+amount,result);
                    break;
                case BetType.Corner:
                    if(bet.Bets.Exists(x=> x==result))
                        return (8*amount+amount,result);
                    break;
                case BetType.Street:
                    if(bet.Bets.Exists(x=> x==result))
                        return (11*amount+amount,result);
                    break;
                case BetType.Trio:
                    if(bet.Bets.Exists(x=> x==result))
                        return (11*amount+amount,result);
                    break;
                case BetType.Basket:
                    if(bet.Bets.Exists(x=> x==result))
                        return (6*amount+amount,result);
                    break;
                case BetType.Low:
                    if(bet.Bets.Exists(x=> x==result))
                        return (amount+amount,result);
                    break;
                case BetType.High:
                    if(bet.Bets.Exists(x=> x==result))
                        return (amount+amount,result);
                    break;
                case BetType.Red:
                    if(IsRed(result))
                        return (amount+amount,result);
                    break;
                case BetType.Black:
                    if(IsBlack(result))
                        return (amount+amount,result);
                    break;
                case BetType.Even:
                    if(IsEven(result))
                        return (amount+amount,result);
                    break;
                case BetType.Odd:
                    if(IsOdd(result))
                        return (amount+amount,result);
                    break;
                case BetType.Snake:
                    if(bet.Bets.Exists(x=> x==result))
                        return (2*amount+amount,result);
                    break;
                case BetType.Column:
                    if(bet.Bets.Exists(x=> x==result))
                        return (2*amount+amount,result);
                    break;
                case BetType.Dozen:
                    if(bet.Bets.Exists(x=> x==result))
                        return (2*amount+amount,result);
                    break;
            }
            return (0,result);
        }

        private bool IsEven(string val)
        {
            if(val == "0" || val =="00")
                return false;
            int i = Int32.Parse(val);
            return (i%2 ==0);
        }
        private bool IsOdd(string val)
        {
            if(val == "0" || val =="00")
                return false;
            return !IsEven(val);
        }
        private bool IsBlack(string val)
        {
            int i = Int32.Parse(val);
            if(i >= 1 && i <= 10)
                return IsEven(i);
            if(i >= 19 && i <= 28)
                return IsEven(i);
            if(i >= 11 && i <= 18)
                return IsOdd(i);
            if(i >= 29 && i <= 36)
                return IsOdd(i);
            return false;
        }
        private bool IsRed(string val)
        {
            if(val == "0"|| val == "00")
                return false;
            return(!IsBlack(val));
        }
        private bool IsEven(int val)
        {
            if(val == 0)
                return false;
            return (val%2 ==0);
        }
        private bool IsOdd(int val)
        {
            if(val == 0)
                return false;
            return (!IsEven(val));
        }

        public Bet GetBet(string betType, string [] bets)
        {
            switch(betType.ToLower())
            {
                case "straight":
                case "single":
                    return new Bet(BetType.Straight,bets);
                case "split":
                    return new Bet(BetType.Split,bets);
                case "street":
                    return new Bet(BetType.Street,bets);
                case "corner":
                case "square":
                    return new Bet(BetType.Corner,bets);
                case "sixline":
                case "six line":
                case "doublestreet":
                case "double street":
                    return new Bet(BetType.Unknown,bets);//not implemented yet
                case "trio":
                    return new Bet(BetType.Trio,bets);
                case "Basket":
                    return new Bet(BetType.Basket,bets);
                case "low":
                    return new Bet(BetType.Low,bets);
                case "high":
                    return new Bet(BetType.High,bets);
                case "even":
                    return new Bet(BetType.Even,bets);
                case "odd":
                    return new Bet(BetType.Odd,bets);
                case "dozen":
                    return new Bet(BetType.Dozen,bets);
                case "column":
                    return new Bet(BetType.Column,bets);
                case "snake":
                    return new Bet(BetType.Snake,bets);
                case "red":
                    return new Bet(BetType.Red,bets);
                case "black":
                    return new Bet(BetType.Black,bets);
                default:
                    return new Bet(BetType.Unknown,bets);
            }
        }
        public bool IsValid(Bet bet)
        {
            switch(bet.Type)
            {
                case BetType.Straight:
                    if(bet.Bets.Count != 1)
                        return false;
                    if(!bet.Bets.TrueForAll(x => Slots.Contains(x)))
                        return false;
                    return true;
                case BetType.Split:
                    if(bet.Bets.Count != 2)
                        return false;
                    if(!bet.Bets.TrueForAll(x => Slots.Contains(x)))
                        return false;
                    if(bet.Bets.Distinct().Count() != bet.Bets.Count)
                        return false;
                    return validSplit(bet.Bets[0],bet.Bets[1]);
                case BetType.Corner:
                    if(bet.Bets.Count != 4)
                        return false;
                    if(!bet.Bets.TrueForAll(x => Slots.Contains(x)))
                        return false;
                    if(bet.Bets.Distinct().Count() != bet.Bets.Count)
                        return false;
                    return validCorner(bet.Bets);
                case BetType.Street:
                    if(bet.Bets.Count != 3)
                        return false;
                    if(!bet.Bets.TrueForAll(x => Slots.Contains(x)))
                        return false;
                    if(bet.Bets.Distinct().Count() != bet.Bets.Count)
                        return false;
                    return validStreet(bet.Bets);
                case BetType.Trio:
                    return validTrio(bet.Bets);
                case BetType.Basket:
                case BetType.Low:
                case BetType.High:
                case BetType.Red:
                case BetType.Black:
                case BetType.Even:
                case BetType.Odd:
                case BetType.Snake:
                    return true; //no input to validate
                case BetType.Column:
                    return validColumn(bet.Bets);
                case BetType.Dozen:
                    return validDozen(bet.Bets);
                default:
                    return false;
                
                    
            }
        }

        private bool validDozen(List<string> bets)
        {
            if(bets.Count != 1)
                return false;
            return (bets[0]=="1" || bets[0]=="2" || bets[0]=="3");
        }

        private bool validColumn(List<string> bets)
        {
            if(bets.Count != 1)
                return false;
            return (bets[0]=="1" || bets[0]=="2" || bets[0]=="3");
        }

        private bool validTrio(List<string> bets)
        {
            
            if(bets.Count != 3)
                return false;

            if(!bets.TrueForAll(x => Slots.Contains(x)))
                return false;

            if(bets.TrueForAll(x => (x=="0"||x=="1"|| x=="2")))
                return true;
            if(bets.TrueForAll(x => (x=="00"||x=="2"|| x=="3")))
                return true;
            return false;
        }

        private  bool validStreet(List<string> bets)
        {
            int i1 = Int32.Parse(bets[0]);
            int i2 = Int32.Parse(bets[1]);
            int i3 = Int32.Parse(bets[2]);

            //can't street with 0 or 00
            if(i1 == 0 || i2 == 0 || i3 == 0)
                return false;
            
            i1 = (int)((i1 - 1) / 3);
            i2 = (int)((i2 - 1) / 3);
            i3 = (int)((i3 - 1) / 3);

            return (i1 == i2 && i2 == i3);
        }

        private  bool validSplit(string first, string second)
        {
            int i1 = Int32.Parse(first);
            int i2 = Int32.Parse(second);

            //row
            if(i1 == 0 && i2 == 0)
                return true;

            //can't split on 0, 00 and other numbers I think
            else if(i1 == 0 || i2 == 0)
                return false;

            int dif;
            if(i1 > i2)
                dif = i1-i2;
            else
                dif = i2-i1;

            return (dif == 1 || dif == 3);
        }

        private bool validCorner(List<string> bets)
        {   
            int validSplits = 0;
            foreach(List<string> kcomb in StringExtensions.GetKCombs<string>(bets,2).ToList())
            {
                if(validSplits >= 4)
                    return true;
                if(validSplit(kcomb[0],kcomb[1]))
                    validSplits++;
            }
            return validSplits >= 4;
            
        }
    }

}