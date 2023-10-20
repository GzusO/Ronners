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
        private int _bonusCount;
        private int _bonus2Count;
        private int _bonusLowerBound = 100;
        private int _bonusUpperBound = 200;

        public bool Success {get;set;} = true;
        public string ErrorMessage {get;set;}

        public int Balance {get{return _balance;}}
        public int Streak {get{return _streak;}}
        public int BonusCount {get{return _bonusCount;}}
        
        public int DailyBonus {get{return _dailyBonus;}}
        public int InterestBonus {get{return _interestBonus;}}
        public int StreakBonus {get{return _streakBonus;}}

        public int TotalBonus {get {return DailyBonus+InterestBonus+StreakBonus;}}
        public double StreakMulitiplier {get {return _streakRate*(Streak-1);}}
        public double InterestRate {get {return _baseInterestRate;}}

        public int BonusUpperBound{get{return _bonusUpperBound+(_bonusCount*50);}}
        public int BonusLowerBound{get{return _bonusLowerBound+(_bonus2Count);}}
        
        public DailyResult(Random rand, double streakRate = .01, double baseInterestRate = .01, double perExtraInterestRate = .02)
        {
            _rand = rand;
            _streakRate = streakRate;
            _baseInterestRate = baseInterestRate;
            _perExtraInterestRate = perExtraInterestRate;
        }

        public void CalculateDaily(int Balance, int Streak, int Bonus, int Bonus2)
        {
            _balance = Balance;
            _streak = Streak;
            _bonusCount = Bonus;
            _bonus2Count = Bonus2;

            CalculateBonus();
            CalculateStreakBonus();
            CalculateInterest();
        }
        private void CalculateBonus()
        {
            _dailyBonus = _rand.Next(BonusLowerBound,BonusUpperBound);
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