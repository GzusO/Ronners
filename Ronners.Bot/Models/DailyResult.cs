using System;

namespace Ronners.Bot.Models
{
    public class DailyResult
    {
        private double _streakRate;
        private double _baseInterestRate;
        private double _perExtraInterestRate;
        private readonly Random _rand;
        private int _dailyBonus;
        private int _streakBonus;
        private int _interestBonus;
        private int _streak;
        private int _balance;
        private int _extraInterestInstances;

        public bool Success {get;set;} = true;
        public string ErrorMessage {get;set;}

        public int Balance {get{return _balance;}}
        public int Streak {get{return _streak;}}
        public int ExtraInterestInstances {get{return _extraInterestInstances;}}
        
        public int DailyBonus {get{return _dailyBonus;}}
        public int InterestBonus {get{return _interestBonus;}}
        public int StreakBonus {get{return _streakBonus;}}

        public int TotalBonus {get {return DailyBonus+InterestBonus+StreakBonus;}}
        public double StreakMulitiplier {get {return _streakRate*(Streak-1);}}
        public double InterestRate {get {return _baseInterestRate+(_perExtraInterestRate*ExtraInterestInstances);}}
        
        public DailyResult(Random rand, double streakRate = .01, double baseInterestRate = .01, double perExtraInterestRate = .02)
        {
            _rand = rand;
            _streakRate = streakRate;
            _baseInterestRate = baseInterestRate;
            _perExtraInterestRate = perExtraInterestRate;
        }

        public void CalculateDaily(int Balance, int Streak, int ExtraInterestInstances)
        {
            _balance = Balance;
            _streak = Streak;
            _extraInterestInstances = ExtraInterestInstances;

            CalculateBonus();
            CalculateStreakBonus();
            CalculateInterest();
        }
        private void CalculateBonus()
        {
            _dailyBonus = _rand.Next(100,200);
        }

        private void CalculateStreakBonus()
        {
            _streakBonus = (int)Math.Floor(_dailyBonus*StreakMulitiplier);
        }
        
        private void CalculateInterest()
        {
            _interestBonus = (int)Math.Floor(InterestRate*Balance);
        }


    }
}