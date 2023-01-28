using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace Ronners.Bot.Models
{
    public class UserItemDetailed
    {
        public ulong UserID{get;set;}
        public int ItemID{get;set;}
        public int Quantity{get;set;}
        public Item Item{get;set;}

        public UserItemDetailed(UserItem userItem, Item item)
        {
            UserID = userItem.UserID;
            ItemID = userItem.ItemID;
            Quantity = userItem.Quantity;
            Item = item;
        }
    }
}

