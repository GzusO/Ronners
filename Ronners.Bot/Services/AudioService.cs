using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using Discord.Audio;
using System.Collections.Concurrent;
using System;
using System.IO;
using Ronners.Bot.Extensions;
using System.Linq;

namespace Ronners.Bot.Services
{
    public class AudioService
    {

        public string AudioPath{get;set;}
        private readonly ConcurrentDictionary<string,string> AudioFiles = new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private readonly Random _rand;


        public AudioService(Random rand)
        {
            AudioPath = Path.Combine(Directory.GetCurrentDirectory(),ConfigService.Config.AudioFolder);
            _rand = rand;
        }
        public async Task InitializeAsync()
        {
            await GatherAvailableAudioFilesAsync();
        }

        private async Task GatherAvailableAudioFilesAsync()
        {
            foreach(var file in Directory.GetFiles(AudioPath,"*.mp3"))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if(fileName != "ronners")
                    fileName = fileName.Replace("ronners","").ToLower();
                if(AudioFiles.TryAdd(fileName,file));
                    await LoggingService.LogAsync("audio",LogSeverity.Info,$"Added '{file}' with Key: '{fileName}' as valid audio file.");
            }
        }

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                // If you add a method to log happenings from this service,
                // you can uncomment these commented lines to make use of that.
                await LoggingService.LogAsync("audio",LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }
    
        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string audioFile)
        {
            string filePath;
            if(!AudioFiles.TryGetValue(audioFile,out filePath))
            {
                //Check for new files before giving up.
                await GatherAvailableAudioFilesAsync();
                if(!AudioFiles.TryGetValue(audioFile,out filePath))
                    return;

            }

            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                await LoggingService.LogAsync("audio",LogSeverity.Debug, $"Starting playback of {filePath} in {guild.Name}");
                using (var ffmpeg = CreateProcess(filePath))
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    try 
                    {
                        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
                    }
                    catch(Exception e)
                    {
                        await LoggingService.LogAsync("audio",LogSeverity.Error,"Error Occured",e);
                    }
                    finally 
                    {  
                        await stream.FlushAsync();
                    }
                }
            }
        }

        private Process CreateProcess(string filename)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel error -i {filename} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        public string GetRandomAudioFile()
        {
            return AudioFiles.Keys.ToList()[_rand.Next(AudioFiles.Keys.Count)];
        }
        public string[] GetAudioFiles()
        {
            return AudioFiles.Keys.ToArray();
        }
        public bool ValidFile(string audioFile)
        {
            return AudioFiles.ContainsKey(audioFile);
        }
    }
}