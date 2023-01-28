using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;

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
        private PngEncoder _encoder;

        public ImageService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<Discord.WebSocket.DiscordSocketClient>();
            _game = services.GetRequiredService<GameService>();
            _services = services;
            _webService = services.GetRequiredService<WebService>();

            _channels = ConfigService.Config.WhitelistedChannel;
            ImgPath =Path.Combine(Directory.GetCurrentDirectory(),ConfigService.Config.ImgFolder);
            _encoder = new PngEncoder();
            _encoder.ChunkFilter = PngChunkFilter.ExcludeAll;
            _encoder.FilterMethod = PngFilterMethod.Adaptive;
            _encoder.InterlaceMethod =PngInterlaceMode.None;
            _encoder.BitDepth = PngBitDepth.Bit8;
            _encoder.ColorType =PngColorType.Rgb;
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
                    await image.SaveAsPngAsync(Path.Combine(ImgPath,attachment.Filename),_encoder);
                    await message.Channel.SendFileAsync(Path.ChangeExtension(Path.Combine(ImgPath,attachment.Filename),"png"),"Ronners!");
                    image.Dispose();
                }
            }
        }

        public Image<Rgba32> EdgeDetect(Image<Rgba32> image)
        {
            image.Mutate(x => x.DetectEdges(KnownEdgeDetectorKernels.Sobel,false));
            if(image.Height * image.Width * 4 > 8294400)
            {
                if(image.Height > 1080)
                    image.Mutate(x => x.Resize(0,1080));
                else if (image.Width > 1920)
                    image.Mutate(x => x.Resize(1920,0));
            }
                
            return image;
        }
        public Image<Rgba32> EdgeDetect(Stream stream)
        {
            var image = Image.Load<Rgba32>(stream);
            return image;
        }

        public string EdgeDetectAndSave(Stream stream, string fileName)
        {
            var image = Image.Load<Rgba32>(stream);
            image = EdgeDetect(image);
            var filePath = Path.ChangeExtension(Path.Combine(ImgPath,fileName),"png");
            image.SaveAsPngAsync(Path.Combine(filePath),_encoder);
            image.Dispose();
            return filePath;
        }
    }
}