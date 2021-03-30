using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace Ronners.Bot.Services
{
    public class ImageService
    {

        private readonly Discord.WebSocket.DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly WebService _webService;
        private readonly GameService _game;
        private  List<ulong> _channels;
        private static string ImgPath;

        public ImageService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<Discord.WebSocket.DiscordSocketClient>();
            _game = services.GetRequiredService<GameService>();
            _services = services;
            _webService = services.GetRequiredService<WebService>();

            _channels = ConfigService.Config.WhitelistedChannel;
            ImgPath =Path.Combine(Directory.GetCurrentDirectory(),ConfigService.Config.ImgFolder);
        }

        public async Task MessageReceivedAsync(Discord.WebSocket.SocketMessage message)
        {
            if (message.Source != Discord.MessageSource.User)
                return;
            if(!_channels.Contains(message.Channel.Id))
                return;
            if(!message.MentionedUsers.Any(p => p.Id == _discord.CurrentUser.Id))
                return;

            if (message.Attachments.Count > 0)
            {
                foreach(var attachment in message.Attachments)
                {
                    var stream = await _webService.GetFileAsStream(attachment.Url);
                    var image = Image.Load<Rgba32>(stream);
                    EdgeDetect(image);
                    await image.SaveAsync(Path.Combine(ImgPath,attachment.Filename));
                    await message.Channel.SendFileAsync(Path.Combine(ImgPath,attachment.Filename),"Ronners!");
                    image.Dispose();
                }
            }
        }

        public Image<Rgba32> EdgeDetect(Image<Rgba32> image)
        {
            image.Mutate(x => x.DetectEdges(KnownEdgeDetectorKernels.Sobel,false));
            return image;
        }

    }
}