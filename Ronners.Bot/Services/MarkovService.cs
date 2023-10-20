using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Ronners.Bot.Models;

namespace Ronners.Bot.Services
{
    public class MarkovService
    {
        private readonly Discord.WebSocket.DiscordSocketClient _discord;
        private Markov _markovModel;
        private readonly WebService _webService;
        private readonly FishingService _fishingSerivce;

        private readonly Random _rand;
        private readonly KeyService _keyService;

        public MarkovService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<Discord.WebSocket.DiscordSocketClient>();
            _webService =services.GetRequiredService<WebService>();
            _rand = services.GetRequiredService<Random>();
            _fishingSerivce = services.GetRequiredService<FishingService>();
            _markovModel = new Markov("markov.json");
            _keyService = services.GetRequiredService<KeyService>();
        }

        public async Task MessageReceiveAsync(Discord.WebSocket.SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != Discord.MessageSource.User)
                return;
            if(rawMessage.Content.StartsWith("!"))
                return;

            
            addMessageToModel(message.Content);
            if (message.Attachments.Count > 0)
            {
                foreach(var attachment in message.Attachments)
                {
                    if(attachment.Filename.EndsWith(".txt"))
                    {
                        using(var stream = await _webService.GetFileAsStream(attachment.Url))
                        {
                            var reader = new StreamReader(stream);
                            var content = reader.ReadToEnd();
                            addMessageToModel(content);
                        }
                    }
                    
                }
            }

            if(message.Author.Id == 146979125675032576)//Block squirtle
                return;

            if(_rand.Next(0,100)==0 || (message.MentionedUsers.Where(x=> x.Id ==785642924011946004).Count() >0 && message.Author.Id != 997286497411678218))
            {
                string markovMessage = GenerateMessage("");
                markovMessage = _fishingSerivce.Fishify(markovMessage);
                markovMessage = await _keyService.AddKey(markovMessage);
                await message.Channel.SendMessageAsync(markovMessage,false,null,null,null,new Discord.MessageReference(message.Id));
            }
        }

        private void addMessageToModel(string content)
        {
            char[] sep = {' ','\n','\r'};
            var words = content.Split(sep).ToList();
            words.ForEach(x=> x.TrimEnd());
            List<string> lowerWords = new List<string>();
            foreach(var word in words)
            {
                if(!String.IsNullOrWhiteSpace(word))
                    if(!word.StartsWith("http"))
                        lowerWords.Add(word.ToLowerInvariant());
                    else
                    {
                        lowerWords.Add(word);
                    }
            }
            _markovModel.AddToChain(lowerWords.ToList());
        }

        public string GenerateMessage(string start)
        {
            return _markovModel.GenerateString(start);
        }

        public void Purge()
        {
            _markovModel.Purge();
        }

        public Dictionary<string,int> GetPossibleTokens(string token)
        {
            return _markovModel.GetProceedingWords(token);
        }

        internal object GetTokenCount()
        {
            return _markovModel.GetTotalTokens();
        }
    }
}