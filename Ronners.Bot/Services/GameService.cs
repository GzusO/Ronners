using Microsoft.Data.Sqlite;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Ronners.Bot.Models;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Discord.WebSocket;

namespace Ronners.Bot.Services
{
    public class GameService
    {
        public SqliteConnection connection {get;set;}
        private readonly Random _rand;

        public GameService(IServiceProvider services)
        {
            _rand = services.GetRequiredService<Random>();
        }

        public async Task<User> GetUserByID(ulong id)
        {
            return await connection.GetAsync<User>(id);
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await connection.GetAllAsync<User>();
        }

        public async Task<bool> UpdateUser(User user)
        {
            return await connection.UpdateAsync<User>(user);
        }

        public async Task<int> InsertUser(User user)
        {
            var cmd = "INSERT INTO users(userid,username,pogcount,ronpoints) values(@UserId,@Username,@PogCount,@RonPoints)";
            return await connection.ExecuteAsync(cmd, new {UserId = user.UserId, Username = user.Username, PogCount=user.PogCount, RonPoints=user.RonPoints});
        }

        public async Task<int> InsertIdea(Idea idea)
        {
            var cmd = "INSERT INTO ideas(idea,priority) values(@Idea,@Priority)";
            return await connection.ExecuteAsync(cmd, new{Idea = idea.idea, Priority=idea.priority});
        }

        public async Task<int> InsertAchievementMessage(AchievementMessage msg)
        {
            var cmd = "INSERT INTO achievementmessages(AchievementType,IntValue,StringValue,BoolValue,DoubleValue,UserID) values(@Type,@Int,@String,@Bool,@Double,@UserId)";
            return await connection.ExecuteAsync(cmd, new{Type=msg.AchievementType, Int=msg.IntValue, Bool=msg.BoolValue, String=msg.StringValue, Double=msg.DoubleValue, UserId=msg.UserID});
        }

        public async Task<IEnumerable<AchievementMessage>> GetAchievementMessagesByTypeAndUserId(int achievementType, ulong userid)
        {
            var cmd = "SELECT * from achievementmessages where AchievementType=@achievementType AND UserId = @userid";
            return await connection.QueryAsync<AchievementMessage>(cmd,new{achievementType,userid});
        }

        public async Task<bool> HasAchievement(ulong achievementId, ulong userId)
        {
            var cmd = "SELECT count(1) FROM userachievements where UserId = @userId and AchievementId = @achievementId";
            int count = await connection.QuerySingleOrDefaultAsync<int>(cmd, new{achievementId,userId});
            return count != 0;
        }

        public async Task<IEnumerable<Achievement>>GetAchievementsByUserId(ulong userId)
        {
            var cmd = "SELECT ac.* from userachievements ua inner join achievements ac on ac.AchievementId = ua.AchievementId where ua.UserId = @userId";
            return await connection.QueryAsync<Achievement>(cmd,new{userId});
        }

        public async Task<int> InsertUserAchievement(ulong achievementId, ulong userId)
        {
            var cmd = "INSERT INTO userachievements(AchievementId, UserId) values(@achievementId, @userId)";
            return await connection.ExecuteAsync(cmd,new{achievementId,userId});
        }

        public async Task<IEnumerable<Idea>> GetIdeas()
        {
            return await connection.GetAllAsync<Idea>();
        }

        public async Task<Cooldown> GetCooldown(string command)
        {
            return await connection.GetAsync<Cooldown>(command);
        }
        public async Task<bool> UpdateCooldown(Cooldown cooldown)
        {
            return await connection.UpdateAsync<Cooldown>(cooldown);
        }
        public async Task<int> InsertCooldown(Cooldown cooldown)
        {
            var cmd = "INSERT INTO cooldowns(command,lastexecuted) values(@Command,@LastExecuted)";
            return await connection.ExecuteAsync(cmd, new {Command=cooldown.Command, LastExecuted=cooldown.LastExecuted});
        }

        public async Task<int> InsertRetribution(Retribution retrib)
        {
            var cmd = "INSERT INTO retributions(RetributerUserId, RetributeeUserId, Reason, PointsRedistributed, numUsers, Time, Success) values(@RetributerID,@RetributeeID,@Reason,@Points,@Count,@Time,@Success)";
            return await connection.ExecuteAsync(cmd, new {RetributerID = retrib.RetributerUserId, RetributeeID=retrib.RetributeeUserId, Reason=retrib.Reason, Points= retrib.PointsRedistributed, Count=retrib.numUsers, Time=retrib.Time, Success=retrib.Success});
        }

        public async Task<Achievement> GetAchievement(ulong id)
        {
            var cmd = string.Format("SELECT * FROM achievements WHERE achievementid = @id");
            return await connection.QueryFirstAsync<Achievement>(cmd,new{id});
        }

        public async Task<IEnumerable<Retribution>> GetRetributions()
        {
            var cmd = string.Format("SELECT * FROM retributions");
            return await connection.QueryAsync<Retribution>(cmd);
        }

        
        public async Task<IEnumerable<UserRonStock>> GetUserRonStockByUserAsync(IUser user)
        {
            var cmd = "SELECT * FROM userronstock WHERE userid = @id";
            return await connection.QueryAsync<UserRonStock>(cmd,new{id = user.Id});
        }

        public async Task<UserRonStock> GetUserRonStockAsync(IUser user, string symbol)
        {
            var id = user.Id;

            var cmd = string.Format("SELECT * FROM userronstock WHERE userid = @id and symbol = @symbol");
            return await connection.QueryFirstOrDefaultAsync<UserRonStock>(cmd,new{id,symbol});
        }

        public async Task UpdateUserRonStock(UserRonStock userRonStock)
        {
            var cmd = string.Format("UPDATE userronstock SET quantity = @quantity WHERE userid = @id and symbol = @symbol");
            await connection.QueryAsync(cmd,new{quantity = userRonStock.Quantity, id = userRonStock.UserID, symbol = userRonStock.Symbol});
        }

        public async Task<UserDaily> GetUserDaily(IUser user)
        {
            var id = user.Id;

            var cmd = string.Format("SELECT * FROM userdaily WHERE userid=@UserId");
            return await connection.QueryFirstOrDefaultAsync<UserDaily>(cmd,new{UserId=id});
        }

        public async Task AddUserDaily(UserDaily userDaily)
        {
            var cmd = "INSERT INTO userdaily(userid,lastcheckin,streak) values(@UserId,@LastCheckIn,@Streak)";
            await connection.ExecuteAsync(cmd, new{UserId=userDaily.UserID,LastCheckIn=userDaily.LastCheckIn,Streak=userDaily.Streak});
        }
        public async Task UpdateUserDaily(UserDaily userDaily)
        {
            var cmd = "UPDATE userdaily SET lastcheckin = @LastCheckIn, streak = @Streak WHERE userid = @UserId";
            await connection.ExecuteAsync(cmd,new{UserId=userDaily.UserID,LastCheckIn=userDaily.LastCheckIn,Streak=userDaily.Streak});
        }

        public async Task DeleteUserRonStock(UserRonStock userRonStock)
        {
            //Don't Delete if quantity isn't 0
            if(userRonStock.Quantity != 0)
                return;

            var cmd = string.Format("DELETE FROM userronstock WHERE userid = @id and symbol = @symbol");
            await connection.QueryAsync(cmd,new{quantity = userRonStock.Quantity, id = userRonStock.UserID, symbol = userRonStock.Symbol});
        }
        public async Task AddUserRonStock(UserRonStock userRonStock)
        {
            var cmd = "INSERT INTO userronstock(userid,symbol,quantity) values(@UserId,@Symbol,@Quantity)";
            await connection.ExecuteAsync(cmd, new {UserId=userRonStock.UserID, Symbol=userRonStock.Symbol,Quantity = userRonStock.Quantity});
        }

        public async Task<bool> AddRonPoint(IUser user)
        {
            
            return await AddRonPoints(user,1);
        }
        public async Task<bool> AddRonPoints(IUser user, int amount)
        {
            User caller = await GetUserByID(user.Id);
            if(caller is null)
            {
                if(amount < 0)
                    return false;
                await InsertUser(new User(user.Id,user.Username,0,amount));
            }
            else
            {
                if(amount<0 && amount*-1 > caller.RonPoints)
                    return false;
                caller.RonPoints += amount;
                await UpdateUser(caller);
            }
            return true;
        }

        public async Task<RonState>GetRonState()
        {
            var cmd = "SELECT * FROM ronstate";
            return await connection.QueryFirstAsync<RonState>(cmd);
        }

        public async Task<IEnumerable<(ulong threadId, ulong guildId)>> GetRonThreads()
        {
            var cmd = "SELECT DISTINCT threadid, guildid FROM ronthread";
            return await connection.QueryAsync<(ulong,ulong)>(cmd);
        }
        public async Task AddRonThread(ulong threadId,ulong guildId)
        {
            var cmd = "INSERT INTO ronthread(threadid,guildid) values(@threadId,@guildId)";
            await connection.ExecuteAsync(cmd,new{threadId,guildId});
        }
        public async Task RemoveRonThread(ulong threadId)
        {
            var cmd = "DELETE FROM ronthread WHERE threadid = @threadId";
            await connection.ExecuteAsync(cmd,new{threadId});
        }

        public async Task<IEnumerable<Collection>> GetCollections()
        {
            var cmd = "SELECT DISTINCT Collection as Name, Count() as NumberOfItems from item GROUP BY Collection";
            return await connection.QueryAsync<Collection>(cmd);
        }
        
        public async Task<IEnumerable<Item>> GetItemsByCollection(string collection)
        {
            var cmd = "SELECT * FROM item WHERE lower(collection) = lower(@collection)";
            return await connection.QueryAsync<Item>(cmd,new{collection});
        }

        public async Task AddUserItem(UserItem userItem)
        {
            var cmd = "INSERT INTO useritem(userid,itemid,quantity) values(@UserID,@ItemID,@Quantity)";
            await connection.ExecuteAsync(cmd, new{UserID=userItem.UserID, ItemID =userItem.ItemID, Quantity=userItem.Quantity});
        }

        public async Task UpdateUserItem(UserItem userItem)
        {
            var cmd = "UPDATE userItem SET quantity = @Quantity WHERE userid = @UserID AND itemid = @ItemID";
            await connection.ExecuteAsync(cmd, new {Quantity=userItem.Quantity,UserID=userItem.UserID, ItemID=userItem.ItemID});
        }

        public async Task AddItemToUser(Item item, IUser user)
        {
            var userItem = await GetUserItem(item.ItemID,user.Id);
            if(userItem == null)
            {
                userItem = new UserItem(){
                    UserID = user.Id,
                    ItemID = item.ItemID,
                    Quantity = 1
                };
                await AddUserItem(userItem);
            }
            //User already has the item increment quantity and update record.
            else
            {
                userItem.Quantity++;
                await UpdateUserItem(userItem);
            }
        }

        public async Task<UserItem> GetUserItem(int ItemID, ulong UserID)
        {
            var cmd = "SELECT * FROM useritem WHERE ItemID=@ItemID AND UserID=@UserID";
            return await connection.QueryFirstOrDefaultAsync<UserItem>(cmd, new{ItemID,UserID});
        }

        public async Task<IEnumerable<UserItemDetailed>> GetUsersItems(IUser user)
        {
            var cmd = "SELECT * FROM useritem WHERE UserID=@UserID";
            var userItems = await connection.QueryAsync<UserItem>(cmd,new{UserID=user.Id});
            var items = await GetItems();

            var userItemsDetailed = new List<UserItemDetailed>();
            foreach(var userItem in userItems)
            {
                userItemsDetailed.Add(new UserItemDetailed(userItem,items.First(x=> x.ItemID==userItem.ItemID)));
            }
            return userItemsDetailed;
        }

        public async Task<IEnumerable<Item>> GetItems()
        {
            var cmd ="SELECT * FROM item";
            return await connection.QueryAsync<Item>(cmd);
        }

        public async Task<IEnumerable<Item>> PurchaseCollection(string collection, int quantity, IUser user)
        {
            var items = await GetItemsByCollection(collection);
            var totalWeight = items.Sum(x=>x.RarityToWeight());
            var receivedItems = new List<Item>();

            for(int i = 0;i<quantity;i++)
            {
                receivedItems.Add(SelectRandomItem(items,totalWeight));
            }
            foreach(var item in receivedItems)
            {
                await AddItemToUser(item,user);
            }
            return receivedItems;
        }

        public Item SelectRandomItem(IEnumerable<Item> items, int totalWeight)
        {
            var val = _rand.Next(0,totalWeight);

            Item selectedItem = null;
            foreach(var item in items)
            {
                if(val < item.RarityToWeight())
                {
                    selectedItem = item;
                    break;
                }
                val -= item.RarityToWeight();
            }
            return selectedItem;
        }

        public async Task<int> AddRonnersGift(RonGift gift)
        {
            var cmd = "INSERT INTO rongifts(ReceivedDate, ReturnDate, ReceivedPoints, ReturnPoints, UserID, Returned) values(@ReceivedDate, @ReturnDate, @ReceivedPoints, @ReturnPoints, @UserID, @Returned)";
            return await connection.ExecuteAsync(cmd, new {gift.ReceivedDate,gift.ReturnDate,gift.ReceivedPoints,gift.ReturnPoints,gift.UserID,gift.Returned});
        }

        public async Task<IEnumerable<RonGift>> GetRonGiftsNotReturned()
        {
            var cmd = "SELECT * FROM rongifts WHERE Returned = 0";
            return await connection.QueryAsync<RonGift>(cmd);
        }

        public async Task ReturnGift(int ronGiftID)
        {
            var cmd = "UPDATE rongifts SET Returned = 1 WHERE RonGiftID = @ronGiftID";
            await connection.ExecuteAsync(cmd,new {ronGiftID});
        }

        internal async Task<int> GetUserCompletedCollectionCount(IUser user)
        {
            var items = await GetUsersItems(user);
            var collections = await GetCollections();

            int completedCollections = 0;
            foreach(var collection in collections)
            {
                if(items.Where(x=> x.Item.Collection == collection.Name).Count() == collection.NumberOfItems)
                    completedCollections++;
            }

            return completedCollections;

        }
    }
}