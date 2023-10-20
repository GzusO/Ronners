using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Services;
using System.Collections.Generic;
using System;
using System.Linq;
using Ronners.Bot.Extensions;
using Ronners.Bot.Models;
using Discord.WebSocket;
using Ronners.Bot.Models.GoogleBooks;
using System.Text;

namespace Ronners.Bot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<SocketCommandContext>
    {

        public StockService StockService{get;set;}
        public GameService GameService{get;set;}
        public WebService WebService{get;set;}
        public HangmanService HangmanService{get;set;}
        public ReactionService ReactionService{get;set;}
        public Random Rand{get;set;}
        public CaptchaService CaptchaService{get;set;}
        public AudioService AudioService{get;set;}
        public MarkovService MarkovService{get;set;}
        public CommandService _commandService {get;set;}
        public DiscordSocketClient _discord  {get;set;}
        public BookService BookService {get;set;}
        public JellyFinService JellyFinService{get;set;}
        public EconomyService _economyService{get;set;}
        public AchievementService _achievementService{get;set;}

        private static string CaptchaPath = Path.Combine(Directory.GetCurrentDirectory(),ConfigService.Config.CaptchaFolder);

        [Command("help")]
        public async Task Help(int page = 1)
        {
            if(page < 1)
                page = 1;
            var skip = 25*(page-1);
            var module = _commandService.Modules.First(mod => mod.Name=="PublicModule");
            var commands = module.Commands.Skip(skip).Take(25);
            EmbedBuilder embedBuilder = new EmbedBuilder();
            

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";
                embedBuilder.AddField($"{module.Group} {command.Name}", embedFieldText);
            }

            var commandCount = module.Commands.Count();
            int pageCount = (commandCount + 24)/ 25;

            await ReplyAsync($"Commands Page [{page}/{pageCount}]: ", false, embedBuilder.Build());
        }

        [Command("dump")]
        public async Task Dump()
        {
                var messages = Context.Channel.GetMessagesAsync(500);
                var flat = await messages.FlattenAsync();
                foreach(var message in flat)
                    File.AppendAllText("Dump.txt",$"{message.Content}\n");

        }

        [Command("achievements")]
        [Summary("List a users achievements.\nUSAGE: !achievements {USER}")]
        public async Task achievementsAsync(IUser user=null)
        {
            user = user ?? Context.User;
            var achievements = await GameService.GetAchievementsByUserId(user.Id);
            var response = $"{user.Username}'s Achievements ({achievements.Sum(x=>x.Score)})\n";
            foreach(var achievement in achievements)
            {

                if(response.Length + achievement.ToString().Length > 1990)
                {
                    await ReplyAsync(response);
                    response = "";
                }
                response += achievement.ToString()+"\n";
           
            }
            await ReplyAsync(response); 
        }
    
        [Command("best")]
        [Summary("List the Best AMOUNT users.\nUSAGE: !best {AMOUNT}")]
        public async Task bestAsync(int count = 10)
        {
            var allowedMentions = new AllowedMentions(null);
            allowedMentions.MentionRepliedUser=true;

            var users = await GameService.GetUsers();
            var response = "";
            foreach(var user in users.OrderByDescending(p => p.RonPoints).Take(count))
            {
                
                if(response.Length + $"{Discord.MentionUtils.MentionUser(user.UserId)}: {user.RonPoints}".Length > 2000)
                {
                    await ReplyAsync(response);
                    response = "";
                }
                response += $"{Discord.MentionUtils.MentionUser(user.UserId)}: {user.RonPoints}"+"\n";
           
            }
            await ReplyAsync(response,messageReference:Context.Message.Reference,allowedMentions:allowedMentions);
        }


        [Command("captcha")]
        [Summary("Requests a Captcha to solve for points.\nUSAGE: !captcha")]
        public async Task CaptchaAsync()
        {
            DirectoryInfo dir = new DirectoryInfo(CaptchaPath);
            FileInfo[] Files = dir.GetFiles("*.png");

            int index = Rand.Next(Files.Length);

             var message = await Context.Channel.SendFileAsync(Files[index].FullName,"Reply with text in Image.");
             var captchaState = new CaptchaState(message,Path.GetFileNameWithoutExtension(Files[index].Name));
             CaptchaService.AddCaptcha(captchaState);
        }

        [Command("choose")]
        [Summary("Ronners picks the best item.\nUSAGE: !choose {'ITEM1'} {'ITEM2'} ... {'ITEM99'}")]
        public async Task chooseAsync(params string [] options)
        {
            int index = Rand.Next(options.Length);

            await ReplyAsync(string.Format("{0}, Ronners!",options[index]));
        }
        
        [Command("count")]
        [Summary("Displays how many times a USER said the word.\nUSAGE: !count {USER}")]
        public async Task PogCount(IUser user = null)
        {
            user = user ?? Context.User;
            User result;
            ulong pogCount = 0;
            result = await GameService.GetUserByID(user.Id);
            if(result is not null)
            {
                pogCount = result.PogCount;
            }
            string resp = String.Format("{0} has said a variation of p*g {1} times. Ronners!",user.Username,pogCount);
            await ReplyAsync(resp);
            return;
        }

        [Command("daily")]
        [Summary("Daily Rewards.\nUSAGE: !daily")]
        public async Task dailyAsync(string test = "")
        {
            bool testing = !string.IsNullOrWhiteSpace(test);
            var user = Context.User;
            DailyResult result;
            if(testing)
                result = await _economyService.TestDaily(user);
            else
                result = await _economyService.Daily(user);

            if(!result.Success)
            {
                await ReplyAsync(result.ErrorMessage);
                return;
            }

            await ReplyAsync($"{(testing ? "Test Run":"")}",false,CustomEmbeds.BuildEmbed(user,result));

            var achievementResult = AchievementResult.FromSuccess();
            achievementResult.AchievementType = AchievementType.Daily;
            achievementResult.IntValue = result.Streak;
            achievementResult.User = Context.User;
            _achievementService.ProcessMessage(achievementResult);
        }

        [Command("draw")]
        [Summary("Ronners follows commands to draw a image.\nUSAGE: !draw [COMPLICATED PARAMETERS]")]
        public async Task drawAsync([Remainder] string text=null)
        {
            SixLabors.ImageSharp.Color bg;
            string[] commands;
            string file = string.Format("{0}.png",Context.User.Username);
            if(Context.Message.Attachments.Count > 0 && Context.Message.Attachments.First().Filename.EndsWith(".txt"))
            {
                var textFile = await WebService.GetFileAsStream(Context.Message.Attachments.First().Url);
                StreamReader reader = new StreamReader( textFile );
                commands = reader.ReadToEnd().ToLower().Replace("\n","").Replace("\r","").Split(';');
            }
            else
                commands = text.ToLower().Split(';');

            if(commands[0].StartsWith("bg"))
            {
                if(!SixLabors.ImageSharp.Color.TryParse(commands[0].Split(" ")[1],out bg))
                {
                    bg = SixLabors.ImageSharp.Color.White; // Failed Parse Default White
                }
            }
            else
                bg = SixLabors.ImageSharp.Color.White; //not specified default white
            Canvas c = new Canvas(512,512,bg);
            foreach(var command in commands)
            {
                if(command.StartsWith("bg"))
                    continue;
                var args = command.Split(" ");
                switch(args[0])
                {
                    case "p":
                    case "pen":
                        c.SetPenColor(SixLabors.ImageSharp.Color.Parse(args[1]));
                        break;
                    case "l":
                    case "line":
                        c.DrawLine(Int32.Parse(args[1]),Int32.Parse(args[3]),Int32.Parse(args[2]),Int32.Parse(args[4]));
                        break;
                    case "cf":
                    case "circlefill":
                        c.fill = true;
                        c.DrawCircle(Int32.Parse(args[1]),Int32.Parse(args[2]),Int32.Parse(args[3]));
                        c.fill = false;
                        break;
                    case "c":
                    case "circle":
                        c.DrawCircle(Int32.Parse(args[1]),Int32.Parse(args[2]),Int32.Parse(args[3]));
                        break;
                    case "d":
                    case "dot":
                    case "point":
                        c.Draw(Int32.Parse(args[1]),Int32.Parse(args[2]));
                        break;
                    case "rf":
                    case "rectfill":
                    case "rectanglefill;":
                        c.fill = true;
                        c.DrawRectangle(Int32.Parse(args[1]),Int32.Parse(args[2]),Int32.Parse(args[3]),Int32.Parse(args[4]));
                        c.fill = false;
                        break;    
                    case "r":
                    case "rect":
                    case "rectangle":
                        c.DrawRectangle(Int32.Parse(args[1]),Int32.Parse(args[2]),Int32.Parse(args[3]),Int32.Parse(args[4]));
                        break;
                }
            }
            c.Save(file);
            await Context.Channel.SendFileAsync(file,"Ronners!");

            File.Delete(file); //Cleanup
        }

        [Command("give")]
        [Summary("Give RonPoints to a user.\nUSAGE: !give {user} {amount}")]
        public async Task giveAsync(IUser user, int amount)
        {
            var giver = Context.User;
            var allowedMentions = new AllowedMentions(null);
            allowedMentions.MentionRepliedUser=true;

            if(amount <0)
            {
                await ReplyAsync($"Invalid amount. Amount must be > 0");
                return;
            }
            
            //Remove points from Giver
            if(await GameService.AddRonPoints(giver,-1*amount))
            {
                //Grant points to receiver
                await GameService.AddRonPoints(user,amount);
                await ReplyAsync ($"{giver.Mention} gave {amount} RonPoints to {user.Mention}.",allowedMentions:allowedMentions);
            }
            else
            {
                await ReplyAsync($"You do not have enough points.");
                return;
            }
        }
        
        [Command("markov")]
        [Summary("Ronners Reply with a new setence starting with WORD.\nUSAGE: !markov {STARTWORD}")]
        public async Task markovAsync([Remainder] string text = null)
        {
            if(text == null)
                text ="";
            var response = MarkovService.GenerateMessage(text.ToLowerInvariant().Trim());

            var chance = Rand.Next(0,100);
            if(Rand.Next(0,100)==0)
                await ReplyAsync(response.owo());
            else
                await ReplyAsync(response);
        }

        [Command("purge")]
        [Discord.Commands.RequireOwner]
        [Summary("Purges Ronner's Markovian Model, Cost 100rp.\nUSAGE: !purge")]
        public async Task purgeAsync()
        {
            if(!await GameService.AddRonPoints(Context.User,-100))
            {
                await ReplyAsync("Not Enough Points! Costs 100 RonPoints.");
                return;
            }

            MarkovService.Purge();
            await ReplyAsync("Markov Brain Purged, Ronners!");
        }
        [Command("inspect")]
        [Summary("Get details of proceeding tokens in the markov model.\nUSAGE: !inspect {word} {amount}")]
        public async Task markovDump(string token, int count=0)
        {
            var result = MarkovService.GetPossibleTokens(token.ToLowerInvariant().Trim());
            StringBuilder stringBuilder = new StringBuilder();
            
            var listOfTokens = result.ToList().OrderByDescending(x=> x.Value);

            foreach(var kvp in listOfTokens)
            {
                if(stringBuilder.Length < 4000)
                    stringBuilder.Append($"{kvp.Key} : {kvp.Value}\n");
                else
                    break;
            }

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(token.ToLowerInvariant().Trim());
            builder.WithDescription(stringBuilder.ToString());
            await ReplyAsync("",embed:builder.Build());
        }
        [Command("words")]
        [Summary("Get count of unique 'words' in the markov model.\nUSAGE: !words")]
        public async Task markovCount()
        {
            var count = MarkovService.GetTokenCount();
            await ReplyAsync ($"{count} unique tokens in the markov model. Ronners!");    
        }
        
        [Command("userinfo")]
        [Summary("USER Info.\nUSAGE: !userinfo {USER}")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        [Command("source")]
        [Summary("Ronners is Open Source.\nUSAGE: !source")]
        public async Task RonnerSource()
        {
            await ReplyAsync("https://github.com/GzusO/Ronners");
        }

        [Command("points")]
        [Alias("balance")]
        [Summary("Show a USERS points.\nUSAGE: !points {USER}")]
        public async Task PointCount(IUser user = null)
        {
            user = user ?? Context.User;
            User result;
            int points = 0;
            result = await GameService.GetUserByID(user.Id);
            if(result is not null)
            {
                points = result.RonPoints;
            }
            string resp = String.Format("{0} has {1} RonPoints!",user.Username,points);
            await ReplyAsync(resp);
            return;
        }

        // [Command("stock")]
        // [Summary("Lookup a real stock price.\nUSAGE: !stock ['TICKER']")]
        // public async Task stockAsync(string ticker ="")
        // {
        //     var quote = await StockService.GetStockPrice(ticker);
        //     if(quote is null)
        //     {
        //         await ReplyAsync(string.Format("Failed to retrieve stock information for {0}",ticker));
        //     }
        //     await ReplyAsync("",false,CustomEmbeds.BuildEmbed(quote));
        // }

        [Command("book")]
        [Summary("Lookup a book.\nUSAGE: !book ['book query']")]
        public async Task bookAsync([Remainder]string query ="")
        {
            var book = await BookService.GetBookDetails(query);
            if(book is null)
            {
                await ReplyAsync(string.Format("Failed to find a book matching: {0}",query));
            }
            await ReplyAsync("",false,CustomEmbeds.BuildEmbed(book));
        }

        [Command("changelog")]
        [Summary("Maybe an updated Changelog\nUSAGE: !changelog")]
        public async Task changelogAsync()
        {
            string response = "";
            response += @"2021-12-28 18:18 
- Added !daily
- 'Temporarily' Removed !ronstock 
    due to balance issues.
- Reset RonPoints
    due to RonStocks inflating economy.
";
            response += @"2021-10-28 18:48
- Added !slurp
    SLURP!
- Added !markov
    Ronners Random Text Generation
- Added !slots
    Gambling
- Added !baccarat
    More Gambling
- Added !roulette
    MOAR Gambling
- Added new Achievements.
    Goodluck
- Added !ron
    Ronners Video.
- Added !purge 
    to purge the Markovian Model for 100 RonPoints.
- Added !help
    Command List and no descriptions yet.
";
            await ReplyAsync(response);
        }

        [Command("idea")]
        [Summary("Submit an idea for Ronners.\nUSAGE: !idea ['THE IDEA']")]
        public async Task<RuntimeResult> ideaAsync([Remainder] string text)
        {   
            await GameService.InsertIdea(new Idea(text.Replace("'","").Replace("\"",""),Rand.Next()));

            await ReplyAsync("Ronners!");

            var achievementResult = AchievementResult.FromSuccess();

            achievementResult.AchievementType = AchievementType.Ideas;
            achievementResult.User = Context.User;

            return achievementResult;
        }

        [Command("ideas")]
        [Summary("List an AMOUNT of ideas for Ronners.\nUSAGE: !ideas {AMOUNT}")]
        public async Task listIdeaAsync(int count = 5)
        {
            var result = await GameService.GetIdeas();
            string response ="";
            foreach(var idea in result.OrderBy(x => x.priority).Take(count))
            {
                if(response.Length + idea.ToString().Length > 1998)
                {
                    await ReplyAsync(response);
                    response = "";
                }
                response += idea.ToString()+"\n";
            }
            await ReplyAsync(response);
        }   

        [Command("join", RunMode = RunMode.Async)] 
        [Summary("Ronners Joins the Voice Chat.\nUSAGE: !join {CHANNEL}")]
        public async Task JoinAsync(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            var audioFile = AudioService.GetRandomAudioFile();

            await AudioService.JoinAudio(Context.Guild, channel);
            await AudioService.SendAudioAsync(Context.Guild,Context.Channel, audioFile);
        }

        [Command("play", RunMode = RunMode.Async)] 
        [Summary("Ronners Plays some sound.\nUSAGE: !play ['Sound to Play']")]
        public async Task PlayAsync([Remainder]string file)
        {
            file = file.Trim().ToLower();
            if(!AudioService.ValidFile(file))
            {
                await ReplyAsync($"Invalid file: {file}");
                return;
            }
            await ReplyAsync($"Playing {file}.");
            await AudioService.SendAudioAsync(Context.Guild,Context.Channel,file);
        }

        [Command("leave", RunMode = RunMode.Async)] 
        [Summary("Ronners leaves a voice chat.\nUSAGE: !leave")]
        public async Task LeaveAsync()
        {
            await AudioService.LeaveAudio(Context.Guild);
        }
        [Command("audio")]
        [Summary("List audio files available to play.")]
        public async Task AudioAsync()
        {
            var files = AudioService.GetAudioFiles();
            string response = "";
            foreach(var file in files)
            {
                if(response.Length + file.Length > 1998)
                {
                    await ReplyAsync(response);
                    response = "";
                }
                response += file+"\n";
            }
            await ReplyAsync(response);
        }

        [Command("8ball")]
        [Summary("Ronners predicts usings an 8 Ball.\nUSAGE: !8ball ['TEXT']")]
        public async Task<RuntimeResult> DecisionAsync ([Remainder] string text)
        {
            string response="";
            int responseType = 0; // 1=Positive, 0=None, -1= Negative
            var result = Rand.Next() % 15;
            switch(result)
            {
                case 0:
                    response ="As I see it, yes.";
                    responseType =1;
                    break;
                case 1:
                    response ="Most likely.";
                    responseType =1;
                    break;
                case 2:
                    response ="Outlook good.";
                    responseType =1;
                    break;
                case 3:
                    response ="Yes.";
                    responseType =1;
                    break;
                case 4:
                    response ="Signs point to yes.";
                    responseType =1;
                    break;
                case 5:
                    response ="Reply hazy, try again.";
                    responseType =0;
                    break;
                case 6:
                    response ="Ask again later.";
                    responseType =0;
                    break;
                case 7:
                    response ="Better not tell you now.";
                    responseType =0;
                    break;
                case 8:
                    response ="Cannot predict now.";
                    responseType =0;
                    break;
                case 9:
                    response ="Concentrate and ask again.";
                    responseType =0;
                    break;
                case 10:
                    response ="Don't count on it.";
                    responseType =-1;
                    break;
                case 11:
                    response ="My reply is no.";
                    responseType =-1;
                    break;
                case 12:
                    response ="My sources say no.";
                    responseType =-1;
                    break;
                case 13:
                    response ="Outlook not so good.";
                    responseType =-1;
                    break;
                case 14:
                    response ="Very doubtful.";
                    responseType =-1;
                    break;
                default:
                    response ="WTF how did rand % 15 not return a result between 0-14 inclusive";
                    responseType =2;//magic impossible
                    break;
            }
            response+= " Ronners!";
            await ReplyAsync(response);

            var achievementResult = AchievementResult.FromSuccess();

            achievementResult.AchievementType = AchievementType.EightBall;
            achievementResult.IntValue = responseType;
            achievementResult.User = Context.User;

            return achievementResult;
        } 


        [Command("retribute")]
        [Summary("Ronners decides if a USER should lose all their points for a REASON.\nUSAGE: !retribute [USER] ['REASON']")]
        public async Task RetributionAsync(IUser user, [Remainder] string reason)
        {
            int cooldownDuration = 86400;//1 day
            if(user is null)
                return;
            
            Cooldown cd = await GameService.GetCooldown("retribution");

            if(cd is null)
            {  
                cd = new Cooldown("retribution",DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());
                await GameService.InsertCooldown(cd);
            }

            if(cd.LastExecuted + cooldownDuration >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                TimeSpan t = TimeSpan.FromSeconds(cd.LastExecuted+cooldownDuration-DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await ReplyAsync(string.Format("Command on cooldown for {0}",t.ToString(@"hh\:mm\:ss\:fff")));
                return;
            }
            else
            {
                cd.LastExecuted = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await GameService.UpdateCooldown(cd);

                Retribution retrib;
                if(Rand.Next() % 2 == 0)
                {
                    retrib = new Retribution(Context.User.Id, user.Id, reason, 0,0,DateTimeOffset.UtcNow.ToUnixTimeSeconds(),false);
                    await GameService.InsertRetribution(retrib); 
                    await ReplyAsync(string.Format("{0} was not Retributed today, Ronners!",user.Username));
                    return;
                }


                User victim = await GameService.GetUserByID(user.Id);
                int points = victim.RonPoints;
                if(points <= 0)
                {
                    retrib = new Retribution(Context.User.Id, user.Id, reason, 0,0,DateTimeOffset.UtcNow.ToUnixTimeSeconds(),false);
                    await GameService.InsertRetribution(retrib); 
                    await ReplyAsync(string.Format("{0} was not Retributed today, because they have negative points!",user.Username));
                    return;
                }
                victim.RonPoints = 0;
                await GameService.UpdateUser(victim);

                var receivers = await GameService.GetUsers();
                int pointsEach = points/receivers.Count();
                foreach(var receiver in receivers)
                {
                    receiver.RonPoints += pointsEach;
                    await GameService.UpdateUser(receiver);
                }
                
                await GameService.UpdateUser(victim);//Remove any points reassigned to the user;
                retrib = new Retribution(Context.User.Id,user.Id,reason,points,receivers.Count(),DateTimeOffset.UtcNow.ToUnixTimeSeconds(),true);
                await GameService.InsertRetribution(retrib); 
                await ReplyAsync(string.Format("{0} has been retributed, because {1}.\nLosing {2} points. Ronners!",user.Username,reason,points));
            }
                
        }
        
        [Command("retributions")]
        [Summary("List Retribution history.\nUSAGE: !retributions")]
        public async Task RetributionListAsync()
        {
            var retributions = await GameService.GetRetributions();
            var response = "";
            foreach(var retrib in retributions.OrderBy(p => p.Time))
            {

                if(response.Length + retrib.ToString().Length > 2000)
                {
                    await ReplyAsync(response);
                    response = "";
                }
                response += retrib.ToString()+"\n";
           
            }
            await ReplyAsync(response);
        }

        [Command("roll")]
        [Summary("Ronners rolls a die.\nUSAGE: !roll ['1d4']")]
        public async Task<RuntimeResult> RollAsync(string roll = "")
        {
            roll = roll.ToLower();
            int result =0;
            if(roll.Contains('d'))
            {   
                string [] split = roll.Split('d');
                int amount = Int32.Parse(split[0]);
                int die = Int32.Parse(split[1]);
                foreach(var i in Enumerable.Range(0,amount))
                {
                    result += Rand.Next(1,die);
                }
            }
            else if(roll.Contains('-'))
            {
                string [] split = roll.Split('-');
                int lower = Int32.Parse(split[0]);
                int upper = Int32.Parse(split[1]);
                result = Rand.Next(lower,upper+1);
            }
            else
            {
                result =Rand.Next();
            }
            await ReplyAsync($"{result}");

            var achievementResult = AchievementResult.FromSuccess();

            achievementResult.AchievementType = AchievementType.Roll;
            achievementResult.IntValue = result;
            achievementResult.User = Context.User;

            return achievementResult;

        }
        [Command("roll")]
        [Summary("Ronners picks a random number between LOWER and UPPER.\nUSAGE: !roll [LOWER] [UPPER]")]
        public async Task<RuntimeResult> RollAsync(int lower, int upper)
        {
            int result = Rand.Next(lower,upper+1);
            await ReplyAsync($"{result}");
            var achievementResult = AchievementResult.FromSuccess();

            achievementResult.AchievementType = AchievementType.Roll;
            achievementResult.IntValue = result;
            achievementResult.User = Context.User;

            return achievementResult;
        }

        [Command("slurp")]
        [Summary("Ronners slurps.\nUSAGE: !slurp")]
        public async Task<RuntimeResult> SlurpAsync()
        {
            await Context.Channel.SendFileAsync("slurp.mp4","Slurp!");

            var achievementResult = AchievementResult.FromSuccess();

            achievementResult.AchievementType = AchievementType.Slurp;
            achievementResult.User = Context.User;

            return achievementResult;
        }
        [Command("ronners")]
        [Summary("Ronners!\nUSAGE: !ronners")]
        public async Task RonAsync()
        {
            await Context.Channel.SendFileAsync("ron.mp4","Ronners!");
        }

        [Command("Hangman")]
        [Summary("Ronners starts a hangman game.\nUSAGE: !hangman")]
        public async Task HangmanAsync()
        {
            if(HangmanService.Started)
                return;
            var message = await ReplyAsync($"Hangman Starting Soon!");
            HangmanService.StartGame(message);
        }

        [Command("TicTacToe")]
        [Summary("Ronners starts a TicTacToe game for you and USER.\nUSAGE: !tictactoe [USER]")]
        public async Task TicTacToeAsync(IGuildUser user)
        {
            var message = await ReplyAsync($"<@{Context.User.Id}> Challenged <@{user.Id}> to TicTacToe.");
            Emoji[] emotes = new Emoji[9]{new Emoji("\u0031\uFE0F\u20E3"), new Emoji("\u0032\uFE0F\u20E3"), new Emoji("\u0033\uFE0F\u20E3"), new Emoji("\u0034\uFE0F\u20E3"),new Emoji("\u0035\uFE0F\u20E3"), new Emoji("\u0036\uFE0F\u20E3"), new Emoji("\u0037\uFE0F\u20E3"), new Emoji("\u0038\uFE0F\u20E3"), new Emoji("\u0039\uFE0F\u20E3")};
            await message.AddReactionsAsync(emotes);
            var gameState = new TicTacToeGameState(message,Context.User.Id,user.Id);
            ReactionService.AddTicTacToe(gameState);
            await message.ModifyAsync(m => { m.Content = gameState.ToString();});
        }

        [Command("owo")]
        [Alias("uwu")]
        [Summary("Wonnews UwU's some text (✿ ♡‿♡).\nUSAGE: !owo ['TEXT']")]
        public async Task OwOAsync([Remainder]string text)
        {
            if(!await GameService.AddRonPoints(Context.User,-2))
            {
                await ReplyAsync("Not Enough Points! Costs 2 RonPoints.".owo());
                return;
            }
            await ReplyAsync(text.owo());
        }

        [Command("set")]
        [RequireOwner]
        [Summary("Sets activity message")]
        public async Task SetAsync([Remainder]string text)
        {
            await _discord.SetGameAsync(text,null,ActivityType.Playing);
        }


        [Command("inventory")]
        [Alias("inv")]
        [Summary("Display Users inventory of gacha items.\nUSAGE: !inventory")]
        public async Task InventoryAsync(IUser user = null)
        {
            user = user ?? Context.User;
            var items = await GameService.GetUsersItems(user);
            var collections = await GameService.GetCollections();

            var menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Select a Collection")
                .WithCustomId("inventory_details")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach(var col in collections)
                menuBuilder.AddOption(col.Name,col.Name);

            var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);
    
            await Context.Message.ReplyAsync($"{user.Username}'s Gacha Collection",false,CustomEmbeds.BuildEmbed(items,collections),components:builder.Build());
        }

        [Command("test",RunMode = RunMode.Async)]
        public async Task testAsync([Remainder]string id = "")
        {
            if(AudioService.CurrentlyPlaying(Context.Guild))
            {
                await ReplyAsync("Currently playing a song please wait.");
                return;
            }
            Models.JellyFin.Item song;
            if(string.IsNullOrWhiteSpace(id))
                song = await JellyFinService.Random();
            else
                song = await JellyFinService.GetSongByID(id);
            var response = await ReplyAsync("Now Playing:",false, CustomEmbeds.BuildEmbed(song));
            await AudioService.JellyFinPlay(Context.Guild,Context.Channel,song.Id);
            await response.ModifyAsync(x=> x.Content="Finished Playing:");
        }

        [Command("stop")]
        public async Task stopAsyync()
        {
            AudioService.Stop(Context.Guild);
        }

        [Command("completed")]
        public async Task completedAsync()
        {
            int count = await GameService.GetUserCompletedCollectionCount(Context.User);
            await ReplyAsync($"You have completed {count} collections.");
        }
    }
}