
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ronners.Bot.Models
{
    public class Markov
    {
        Dictionary<string,Dictionary<string,int>> Chain;
        System.Random _random;
        public Markov()
        {
            Chain = new Dictionary<string, Dictionary<string, int>>();
            _random = new System.Random();
        }

        public void Purge()
        {
            foreach(var key in Chain.Keys)
            {
                Chain[key].Clear();
            }
            Chain.Clear();
        }
        public void GenerateChain(List<string> words)
        {
            Chain.Clear();

            var tempChain = new Dictionary<string,Dictionary<string,int>>();

            for(int i = 0; i< words.Count-1;i++)
            {
                var lowecaseWord = words[i].ToLowerInvariant();
                var nextWord = words[i+1].ToLowerInvariant();
                if(tempChain.ContainsKey(lowecaseWord))
                {
                   
                    if(tempChain[lowecaseWord].ContainsKey(nextWord))
                        tempChain[lowecaseWord][nextWord]++;
                    else
                        tempChain[lowecaseWord].Add(nextWord,1);
                }
                else
                {
                    tempChain.Add(lowecaseWord, new Dictionary<string, int>());
                    Chain[lowecaseWord].Add(nextWord,1);
                }
            }

            Chain = tempChain;
        }
        public void AddToChain(List<string> words)
        {
            for(int i = 0; i< words.Count-1;i++)
            {
                var lowecaseWord = words[i];
                var nextWord = words[i+1];
                if(Chain.ContainsKey(lowecaseWord))
                {
                   
                    if(Chain[lowecaseWord].ContainsKey(nextWord))
                        Chain[lowecaseWord][nextWord]++;
                    else
                        Chain[lowecaseWord].Add(nextWord,1);
                }
                else
                {
                    Chain.Add(lowecaseWord, new Dictionary<string, int>());
                    Chain[lowecaseWord].Add(nextWord,1);
                }
            }
        }

        public string GenerateString(string start)
        {
            StringBuilder sb = new StringBuilder();
            var normalizedChain = NormalizeChain(Chain);
            
            string key;
            if(normalizedChain.ContainsKey(start))
                key = start;
            else
                key = PickFirst(normalizedChain);
            sb.Append(key);
            sb.Append(" ");
            
            var count = 1;
            while(count < 140)
            {
                key = PickNext(normalizedChain, key);
                sb.Append(key);
                sb.Append(" ");
                if(key == "")
                    return sb.ToString();
                count++;
            }
            return sb.ToString();
            
        }

        private string PickNext(Dictionary<string, Dictionary<string, double>> normalizedChain,string key)
        {
            if(!normalizedChain.ContainsKey(key))
                return "";
            var row = normalizedChain[key];
            var r = _random.NextDouble();
            var n = 0.0;

            foreach (var kvp in row)
            {
                n += kvp.Value;
                if (r < n)
                {
                    return kvp.Key;
                }
            }

            return row.Last().Key;
        }

        private string PickFirst(Dictionary<string, Dictionary<string, double>> normalizedChain)
        {
            var l = normalizedChain.Keys.Count;
            var r = _random.Next(0,l);
            return normalizedChain.Keys.ElementAt(r);
        }

        private Dictionary<string, Dictionary<string, double>> NormalizeChain(Dictionary<string, Dictionary<string, int>> chain)
        {
            var normalizedChain = new Dictionary<string,Dictionary<string,double>>();

            foreach( var kvp in chain)
            {
                var key = kvp.Key;
                var row = kvp.Value;
                var sum = row.Values.Sum();

                var newRow = row.ToDictionary( pair => pair.Key, pair => (double)pair.Value/sum);
                normalizedChain[key] = newRow;
            }

            return normalizedChain;
        }
    }   
}
