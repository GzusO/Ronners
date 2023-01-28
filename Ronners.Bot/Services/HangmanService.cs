using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using Ronners.Bot.Extensions;
namespace Ronners.Bot.Services
{
    public class HangmanService
    {
        private readonly string[] HangmanArt = {
@"```  ┌───┐  
  │   │  
      │  
      │  
      │  
      │  
──────┴──```",
@"```  ┌───┐   
  │   │   
  O   │   
      │   
      │   
      │   
──────┴──```",
@"```  ┌───┐    
  │   │   
  O   │   
  │   │   
      │   
      │   
──────┴──```",
@"```  ┌───┐   
  │   │   
  O   │   
 ╱│   │   
      │   
      │   
──────┴──```",
@"```  ┌───┐   
  │   │   
  O   │   
 ╱│╲  │   
      │   
      │   
──────┴──```",
@"```  ┌───┐   
  │   │   
  O   │   
 ╱│╲  │   
 ╱    │   
      │   
──────┴──```",
@"```  ┌───┐   
  │   │   
  O   │   
 ╱│╲  │   
 ╱ ╲  │   
      │   
──────┴──```"
            };

        private readonly string[] PossiblePhrases = {
            "ronners is cool.",
            "ronners!",
            "don",
            "ron points",
            "rekansen",
            "don is a influencer",
            "going big mode!",
            "what?!",
            "god left me unfinished",
            "*anime noises*",
            "dude?!",
            "jojo's bizarre adventure is ok i guess.",
            "weehaw!",
            "*notices bulge*",
            "buca di beppo is a family style",
            "bad boy teenager club",
            "i'm doing a tutorial",
            "i just took a mad succ on a weed",
            "that was cool when sarah was unironically listening to all star by the famous band smash mouth",
            "the windsor special.",
            "anyone wanna go to sonic?",
            "notice me senpai",
            "worm sign",
            "deluxe stroganoff",
            "bogos binted"
        };
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly GameService _game;
        private IUserMessage currentGame;
        public bool Started;
        private HashSet<char> selectedLetters;
        private string currentState ="";

        private int WrongAttempts;
        private Regex pattern = new Regex("[a-zA-Z]");

        private string Phrase = "";
        public HangmanService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _game = services.GetRequiredService<GameService>();
            _services = services;

            _discord.ReactionAdded += ReactionAddedAsync;
        }

        public async Task InitializeAsync()
        {
            selectedLetters = new HashSet<char>();
            Started = false;
        }

        public async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel,ulong> arg2, SocketReaction arg3)
        {
            if(Started)
            {
                var message = await arg1.GetOrDownloadAsync();
                if(message.Id == currentGame.Id)
                {
                    var c = EmoteToChar(arg3.Emote);
                    selectedLetters.Add(c);
                    //Swap _ with the correct letter.
                    var currentStateCharArray = currentState.ToCharArray();
                    foreach(var index in Phrase.AllIndexesOf(c.ToString()))
                    {
                        currentStateCharArray[index] = c;
                    }
                    if(!Phrase.Contains(c))
                    {
                        WrongAttempts++;
                        if(WrongAttempts >= 6)
                        {
                            Started = false;
                            await currentGame.ModifyAsync(m =>{m.Content = $"Hangman Failed\n{HangmanArt[WrongAttempts]}\n```{currentState}```";});
                            await _discord.SetGameAsync("Ronners!");
                            return;
                        }
                    }
                    currentState = new string(currentStateCharArray);
                    await currentGame.ModifyAsync(m =>{m.Content = $"Hangman Started\n{HangmanArt[WrongAttempts]}\n```{currentState}```";});
                }

                if(currentState ==  Phrase)
                {
                    Started = false;
                    await currentGame.ModifyAsync(m =>{m.Content = $"Hangman Completed\n{HangmanArt[WrongAttempts]}\n```{currentState}```";});
                    await _game.AddRonPoint(_discord.GetUser(arg3.UserId));
                    await _discord.SetGameAsync("Ronners!");
                }
            }
        }

        public async void StartGame(IUserMessage message)
        {
            var rand = new Random();
            Phrase = PossiblePhrases[rand.Next(PossiblePhrases.Length)];
            selectedLetters.Clear();
            currentGame = message;
            Started =true;
            WrongAttempts = 0;
            currentState = pattern.Replace(Phrase,"-");
            await _discord.SetGameAsync("Hangman");
            await message.ModifyAsync(m => { m.Content = $"Hangman Started\n{HangmanArt[WrongAttempts]}\n```{currentState}```"; });
        }

        private static char EmoteToChar(IEmote emote)
        {
            switch(emote.Name)
            {
                case "\uD83C\uDDE6":
                    return 'a';
                case "\uD83C\uDDE7":
                    return 'b';
                case "\uD83C\uDDE8":
                    return 'c';
                case "\uD83C\uDDE9":
                    return 'd';
                case "\uD83C\uDDEA":
                    return 'e';
                case "\uD83C\uDDEB":
                    return 'f';
                case "\uD83C\uDDEC":
                    return 'g';
                case "\uD83C\uDDED":
                    return 'h';
                case "\uD83C\uDDEE":
                    return 'i';
                case "\uD83C\uDDEF":
                    return 'j';
                case "\uD83C\uDDF0":
                    return 'k';
                case "\uD83C\uDDF1":
                    return 'l';
                case "\uD83C\uDDF2":
                    return 'm';
                case "\uD83C\uDDF3":
                    return 'n';
                case "\uD83C\uDDF4":
                    return 'o';
                case "\uD83C\uDDF5":
                    return 'p';
                case "\uD83C\uDDF6":
                    return 'q';
                case "\uD83C\uDDF7":
                    return 'r';
                case "\uD83C\uDDF8":
                    return 's';
                case "\uD83C\uDDF9":
                    return 't';
                case "\uD83C\uDDFA":
                    return 'u';
                case "\uD83C\uDDFB":
                    return 'v';
                case "\uD83C\uDDFC":
                    return 'w';
                case "\uD83C\uDDFD":
                    return 'x';
                case "\uD83C\uDDFE":
                    return 'y';
                case "\uD83C\uDDFF":
                    return 'z';
                default:
                    return ' ';
            }
        }
    }
}