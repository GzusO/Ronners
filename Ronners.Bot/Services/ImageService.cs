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
        private readonly Random _rand;

        public ImageService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<Discord.WebSocket.DiscordSocketClient>();
            _game = services.GetRequiredService<GameService>();
            _services = services;
            _webService = services.GetRequiredService<WebService>();
            _rand = services.GetRequiredService<Random>();

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
            if(message.Author.Id == 146979125675032576)//Block squirtle
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

        public Image<Rgba32> Rotate(Image<Rgba32> image)
        {
            image.Mutate(x=> x.Rotate(RotateMode.Rotate90));
            return image;
        }

        public Image<Rgba32> Blur(Image<Rgba32> image)
        {
            image.Mutate(x=> x.GaussianBlur((float)_rand.NextDouble()));
            return image;
        }

        public Image<Rgba32> Pixelate(Image<Rgba32> image)
        {
            image.Mutate(x=> x.Pixelate(_rand.Next(1,4)));
            return image;
        }

        public Image<Rgba32> Hue(Image<Rgba32> image)
        {
            image.Mutate(x=> x.Hue(_rand.Next(0,361)));
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

        internal string Ronify(Stream stream, string fileName)
        {
            var image = Image.Load<Rgba32>(stream);
            var iterations = _rand.Next(3,8);

            for(int i = 0;i<iterations;i++)
            {
                var task = _rand.Next(5);

                switch (task)
                {
                    case 0:
                        image = EdgeDetect(image);
                        break;
                    case 1:
                        image = Rotate(image);
                        break;
                    case 2:
                        image = Blur(image);
                        break;
                    case 3:
                        image = Pixelate(image);
                        break;
                    case 4:
                        image = Hue(image);
                        break;
                    default:
                        break;
                }
            }
            var filePath = Path.ChangeExtension(Path.Combine(ImgPath,fileName),"png");
            image.SaveAsPngAsync(Path.Combine(filePath),_encoder);
            image.Dispose();
            return filePath;
        }
    }
}