using System;

namespace Ronners.Bot.Services
{

    public class Slot
    {
        public char FirstReel{get;set;}
        public char SecondReel{get;set;}
        public char ThirdReel{get;set;}
        private Random _rand;
        private int _multiplier;

        public void Initialize(Random random)
        {
            _rand = random;
        }
        public void Spin()
        {
            FirstReel = GetReelResult();
            SecondReel = GetReelResult();
            ThirdReel = GetReelResult();
        }

        private char GetReelResult()
        {
            switch(_rand.Next(20))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    return '■';
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    return '♢';
                case 12:
                case 13:
                case 14:
                case 15:
                    return '♧';
                case 16:
                case 17:
                case 18:
                    return '♡';
                case 19:
                    return 'ඞ';
                default:
                    return ' ';
            }
        }
        public int Outcome()
        {
            int payout;

            if(FirstReel == '■' && FirstReel == SecondReel && SecondReel == ThirdReel)
            {
                payout=2;       //■ ■ ■
            }
            else if(FirstReel == '♢' && FirstReel == SecondReel && SecondReel == ThirdReel)
            {   
                payout=5;       //♢ ♢ ♢
            }
            else if(FirstReel == '♧' && FirstReel == SecondReel && SecondReel == ThirdReel)
            {
                payout=8;       //♧ ♧ ♧
            }
            else if(FirstReel == '♡' && FirstReel == SecondReel && SecondReel == ThirdReel)
            {
                payout=10;      //♡ ♡ ♡
            }
            else if(FirstReel == 'ඞ' && FirstReel == SecondReel && SecondReel == ThirdReel)
            {
                payout=769;     //ඞ ඞ ඞ
            }
            else if(FirstReel == '■' && FirstReel == SecondReel)
            {
                payout=1;         //■ ■                 
            }
            else if(FirstReel == '♢' && FirstReel == SecondReel )
            {
                payout=2;       //♢ ♢
            }
            else if(FirstReel == '♧' && FirstReel == SecondReel )
            {
                payout=3;       //♧ ♧
            }
            else if(FirstReel == '♡' && FirstReel == SecondReel )
            {
                payout=4;       //♡ ♡
            }
            else if(FirstReel == 'ඞ' && FirstReel == SecondReel )
            {
                payout=20;      //ඞ ඞ
            }
            else
            {
                payout =0;      //Loss
            }
            _multiplier= payout;
            return payout;
        }

        public override string ToString()
        {
            return 
$@"
          .-------. 
       oO[  {_multiplier:D3} X  ]Oo
       .=============.  __
       |             | (  )
       | [{FirstReel}] [{SecondReel}] [{ThirdReel}] | ||
       |             |  ||
       | ■ ■ ■ ::::: |__||
       | ඞඞඞ  ::::: |---|
       | ♡♡♡  ::::: |
       | ♧♧♧  ::::: |
       | ♢♢♢  ::::: |
       |      __ === |
       |_____/__\____|
      /###############\
     /#################\
    |###################|
";
        }
    }

    public class SlotService
    {
        private readonly Random _rand;
        public SlotService(Random rand)
            => _rand = rand;

        public (int,Slot) Play(int amount)
        {
            Slot slot = new Slot();
            slot.Initialize(_rand);
            slot.Spin();

            int payout = slot.Outcome();

            if(payout == 0)
                return (0,slot);
            
            return ((payout * amount) + amount,slot);
        }
        

    }
}