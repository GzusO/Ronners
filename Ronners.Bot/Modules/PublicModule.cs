using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Services;
using System.Collections.Generic;
using System;
using System.Linq;
using Ronners.Bot.Extensions;
using Ronners.Loot;
using Ronners.Bot.Models;

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

        private static string CaptchaPath = Path.Combine(Directory.GetCurrentDirectory(),ConfigService.Config.CaptchaFolder);



        private readonly Dictionary<string,string> Soundbytes = new Dictionary<string,string>()
        {
            {"what", "ronnersWhat.mp3"},
            {"ronners", "ronners.mp3"},
            {"drank", "ronnersDrank.mp3"},
            {"mode", "ronnersMode.mp3"},
            {"ruler", "ronnersRuler.mp3"}
        };

        [Command("help")]
        public async Task Help(int page = 1)
        {
            if(page < 1)
                page = 1;
            var skip = 25*(page-1);
            List<CommandInfo> commands = _commandService.Commands.Skip(skip).Take(25).ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";
                embedBuilder.AddField($"{command.Name}", embedFieldText);
            }

            var commandCount = _commandService.Commands.Count();
            int pageCount = (commandCount + 24)/ 25;

            await ReplyAsync($"Commands Page [{page}/{pageCount}]: ", false, embedBuilder.Build());
            
        }

        [Command("achievements")]
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

        [Command("markov")]
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
    
        [Command("best")]
        public async Task bestAsync(int count = 10)
        {
            var users = await GameService.GetUsers();
            var response = "";
            foreach(var user in users.OrderByDescending(p => p.RonPoints).Take(count))
            {

                if(response.Length + user.PointString().Length > 2000)
                {
                    await ReplyAsync(response);
                    response = "";
                }
                response += user.PointString()+"\n";
           
            }
            await ReplyAsync(response);
        }

        [Command("captcha")]
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
        public async Task chooseAsync(params string [] options)
        {
            int index = Rand.Next(options.Length);

            await ReplyAsync(string.Format("{0}, Ronners!",options[index]));
        }
        
        [Command("count")]
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

        [Command("draw")]
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
        

        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        [Command("source")]
        public async Task RonnerSource()
        {
            await ReplyAsync("https://github.com/GzusO/Ronners");
        }

        [Command("points")]
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

        [Command("stock")]
        public async Task stockAsync(string ticker ="")
        {
            var quote = await StockService.GetStockPrice(ticker);
            if(quote is null)
            {
                await ReplyAsync(string.Format("Failed to retrieve stock information for {0}",ticker));
            }
            await ReplyAsync("",false,BuildEmbed(quote));
        }

        [Command("attribution")]
        public async Task attributionAsync()
        {
            string response = string.Format("Stock Data provided by IEX Cloud https://iexcloud.io");
            await ReplyAsync(response);
        }

        [Command("changelog")]
        public async Task changelogAsync()
        {
            string response = "";
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
            response += @"2021-02-02 20:09 pm
- !draw improved
    c/circle centerX centerY radius - draws circle
    cf/circlefill centerX centerY radius  - draws filled circle
    r/rect/rectangle topLeftX TopLeftY width height - draw rectangle
    rf/rectfill/rectanglefill topLeftx TopLeftY width height - draw filled rectangle
    d/dot/point X Y - draw single point
    Canvas now 512 x 512 in size
";
            response += @"2021-01-30 15:02 pm
- Added !draw
    Canvas is 500x500 max.
    bg {color name/hexstring} - Set background needs to be first cmd defaults to white if not set
    p/pen {color name/hexstring} - Set Pen color for subsequent draws
    l/line x0 y0 x1 y1 -draws line
";
            response += @"2021-01-29 13:20 pm
- Added !idea to submit ideas to ronners
";
            response += @"2021-01-29 13:02 pm\n 
- Fixed !8ball not awarding RonPoints.
- added !changelog.
- added !attribution
";
            await ReplyAsync(response);
        }

        [Command("idea")]
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
        public async Task JoinAsync(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            var filename = Soundbytes.Values.ToList()[Rand.Next(Soundbytes.Values.Count)];

            await AudioService.JoinAudio(Context.Guild, channel);
            await AudioService.SendAudioAsync(Context.Guild,Context.Channel, filename);
        }

        [Command("play", RunMode = RunMode.Async)] 
        public async Task PlayAsync([Remainder]string file)
        {
            string filename;
            if(!Soundbytes.TryGetValue(file.ToLower().Trim(),out filename))
            {
                 await ReplyAsync("Invalid Sound Bite");
                 return;
            }
            await ReplyAsync($"Playing {filename}.");
            await AudioService.SendAudioAsync(Context.Guild,Context.Channel,filename);
        }

        [Command("leave", RunMode = RunMode.Async)] 
        public async Task LeaveAsync()
        {
            await AudioService.LeaveAudio(Context.Guild);
        }

        [Command("8ball")]
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
        public async Task<RuntimeResult> SlurpAsync()
        {
            await Context.Channel.SendFileAsync("slurp.mp4","Slurp!");

            var achievementResult = AchievementResult.FromSuccess();

            achievementResult.AchievementType = AchievementType.Slurp;
            achievementResult.User = Context.User;

            return achievementResult;
        }
        [Command("ron")]
        public async Task RonAsync()
        {
            await Context.Channel.SendFileAsync("ron.mp4","Ronners!");
        }

        [Command("Hangman")]
        public async Task HangmanAsync()
        {
            if(HangmanService.Started)
                return;
            var message = await ReplyAsync($"Hangman Starting Soon!");
            HangmanService.StartGame(message);
        }

        [Command("TicTacToe")]
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
        public async Task OwOAsync([Remainder]string text)
        {
            if(!await GameService.AddRonPoints(Context.User,-2))
            {
                await ReplyAsync("Not Enough Points! Costs 2 RonPoints.".owo());
                return;
            }
            await ReplyAsync(text.owo());
        }

        private Embed BuildEmbed(Item item)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(item.ToString());
            if(item is Weapon)
                builder.AddField((item as Weapon).GetMod(), item.Value);
            if(item is Armor)
                builder.AddField((item as Armor).GetMod(), item.Value);
            foreach(Prefix pre in item.GetPrefixes())
            {
                builder.AddField(pre.Modifer,pre.Value,true);
            }
            foreach(Suffix suf in item.GetSuffixes())
            {
                builder.AddField(suf.Modifer,suf.Value,true);
            }
            builder.WithColor(Color.Purple);
            return builder.Build();
        }

        private Embed BuildEmbed(StockQuoteIEX quote)
        {
            EmbedBuilder builder = new EmbedBuilder();
            bool Up = quote.ChangePercent < 0;
            builder.WithTitle(quote.CompanyName);
            builder.AddField("Ticker",quote.Symbol);
            //builder.AddField("Price",string.Format("${0} \n{1} ({2}%)",quote.LatestPrice,quote.Change,quote.ChangePercent));
            builder.AddField("Price",string.Format("{0:c4}",quote.LatestPrice),true);
            builder.AddField("Change",string.Format("{0:0.00#################}",quote.Change),true);
            builder.AddField("Percent Change",string.Format("{0}%",quote.ChangePercent),true);
            builder.WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds((Int64)quote.LatestUpdate));
            builder.WithFooter("Stock Data provided by IEX Cloud https://iexcloud.io");
            if(quote.ChangePercent < 0)
                builder.WithColor(Color.Red);
            else if(quote.ChangePercent > 0)
                builder.WithColor(Color.Green);
            else
                builder.WithColor(Color.LightGrey);
            return builder.Build();
        }
    }
}