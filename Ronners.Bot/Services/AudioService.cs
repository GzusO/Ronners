using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using Discord.Audio;
using System.Collections.Concurrent;
using System;
using System.IO;
using Ronners.Bot.Extensions;
using System.Linq;
using System.Threading;

namespace Ronners.Bot.Services
{
    public class AudioService
    {

        public string AudioPath{get;set;}
        private readonly ConcurrentDictionary<string,string> AudioFiles = new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> TokenSources = new ConcurrentDictionary<ulong, CancellationTokenSource>();
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
                if(AudioFiles.TryAdd(fileName,file))
                    await LoggingService.LogAsync("audio",LogSeverity.Info,$"Added '{Path.GetFileName(file)}' with Key: '{fileName}' as valid audio file.");
            }
        }

        public bool CurrentlyPlaying(IGuild guild)
        {
            return TokenSources.ContainsKey(guild.Id);
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
            CancellationTokenSource tokenSource;

            if(TokenSources.TryRemove(guild.Id, out tokenSource))
            {
                tokenSource.Cancel();
            }
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                
                await client.StopAsync();
                await LoggingService.LogAsync("audio",LogSeverity.Info,$"Disconnected from voice on {guild.Name}.");
            }
        }

        public void Stop(IGuild guild)
        {
            CancellationTokenSource tokenSource;
            if(TokenSources.TryRemove(guild.Id, out tokenSource))
                tokenSource.Cancel();
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

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            if(!TokenSources.TryAdd(guild.Id,tokenSource))
            {
                await LoggingService.LogAsync("audio",LogSeverity.Info,"Playback already started.");
                return;
            }

            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                
                await LoggingService.LogAsync("audio",LogSeverity.Debug, $"Starting playback of {filePath} in {guild.Name}");
                using(var ffmpeg = CreateProcess(filePath))
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    try 
                    {
                        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream,tokenSource.Token);
                    }
                    catch (OperationCanceledException ex)
                    {
                        await LoggingService.LogAsync("audio",LogSeverity.Info,$"Cancelled playback of {filePath}");
                    }
                    catch(Exception e)
                    {
                        await LoggingService.LogAsync("audio",LogSeverity.Error,"",e);
                    }
                    finally 
                    {  
                        await LoggingService.LogAsync("audio",LogSeverity.Info,$"Playback of {filePath} ended.");
                        await stream.FlushAsync();
                        TokenSources.TryRemove(guild.Id,out _);
                        tokenSource = null;
                        ffmpeg.Kill();
                    }
                }
            }
        }
        public async Task JellyFinPlay(IGuild guild, IMessageChannel channel, string audioFile)
        {
            string id = "987cbed73f0ccd2a5af317d16ab0e843";
            if(!string.IsNullOrWhiteSpace(audioFile))
                id = audioFile;
            string filePath = $"http://192.168.10.155:8096/Audio/{id}/stream?static=true";

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            if(!TokenSources.TryAdd(guild.Id,tokenSource))
            {
                await LoggingService.LogAsync("audio",LogSeverity.Info,"Playback already started.");
                return;
            }

            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                
                await LoggingService.LogAsync("audio",LogSeverity.Debug, $"Starting playback of {filePath} in {guild.Name}");
                using(var ffmpeg = CreateProcess(filePath))
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    try 
                    {
                        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream,tokenSource.Token);
                    }
                    catch (OperationCanceledException ex)
                    {
                        await LoggingService.LogAsync("audio",LogSeverity.Info,$"Cancelled playback of {filePath}");
                    }
                    catch(Exception e)
                    {
                        await LoggingService.LogAsync("audio",LogSeverity.Error,"",e);
                    }
                    finally 
                    {  
                        await LoggingService.LogAsync("audio",LogSeverity.Info,$"Playback of {filePath} ended.");
                        await stream.FlushAsync();
                        TokenSources.TryRemove(guild.Id,out _);
                        tokenSource = null;
                        ffmpeg.Kill();
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