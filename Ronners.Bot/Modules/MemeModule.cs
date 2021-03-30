using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Ronners.Bot.Services;


namespace Ronners.Bot.Modules
{
    [Group("turn")]
    public class MemeModule : ModuleBase<SocketCommandContext>
    {

        public AudioService AudioService{get;set;}

        [Command("down", RunMode = RunMode.Async)]
        public async Task DownAsync(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            await ReplyAsync("For What!");

            if(channel is null)
                return;

            await AudioService.JoinAudio(Context.Guild, channel);
            await AudioService.SendAudioAsync(Context.Guild,Context.Channel, "ronnersWhat.mp3");
            await AudioService.LeaveAudio(Context.Guild);
            
        }

        [Command("up", RunMode = RunMode.Async)]
        public async Task UpAsync(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            await ReplyAsync("Drank!");

            if(channel is null)
                return;

            
            await AudioService.JoinAudio(Context.Guild, channel);
            await AudioService.SendAudioAsync(Context.Guild,Context.Channel, "ronnersDrank.mp3");
            await AudioService.LeaveAudio(Context.Guild);
        }
    }
}