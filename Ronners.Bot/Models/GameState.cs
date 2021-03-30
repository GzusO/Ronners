using Discord;


namespace Ronners.Bot.Models
{
    public class GameState
    {
         public IUserMessage gameMessage{get;set;}

         public GameState(IUserMessage message)
         {
             gameMessage= message;
         }
    }
}