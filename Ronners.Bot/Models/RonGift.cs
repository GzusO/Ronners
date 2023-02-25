using System;
using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    [Table("rongifts")]
    public class RonGift
    {
        public int RonGiftID{get;set;}
        public ulong UserID{get;set;}
        public long ReturnDate {get;set;}
        public long ReceivedDate {get;set;}
        public int ReceivedPoints{get;set;}
        public int ReturnPoints{get;set;}
        public int Returned{get;set;}

        public RonGift(ulong user, int points, long time)
        {
            UserID = user;
            ReceivedPoints = points;
            ReceivedDate = time;
            Returned = 0;
        }
        public RonGift()
        {
            //Required By Dapper
        }

    }
}

