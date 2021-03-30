using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Ronners.Bot.Extensions
{
    static class StringExtensions
    {
        private static string[] suffixes = {
  " ( ͡° ᴥ ͡°)",
  " (´・ω・｀)",
  " (ʘᗩʘ\")",
  " (இωஇ )",
  " (๑•́ ₃ •̀๑)",
  " (• o •)",
  " (⁎˃ᆺ˂)",
  " (╯﹏╰）",
  " (●´ω｀●)",
  " (◠‿◠✿)",
  " (✿ ♡‿♡)",
  " (❁´◡`❁)",
  " (　\"◟ \")",
  " (人◕ω◕)",
  " (；ω；)",
  " (｀へ´)",
  " ._.",
  " :3",
  " :D",
  " :P",
  " ;-;",
  " ;3",
  " ;_;",
  " <{^v^}>",
  " >_<",
  " >_>",
  " UwU",
  " XDDD",
  " ^-^",
  " ^_^",
  " x3",
  " x3",
  " xD",
  " ÙωÙ",
  " ʕʘ‿ʘʔ",
  " ㅇㅅㅇ",
  ", fwendo",
  "（＾ｖ＾）",
        };
        public static IEnumerable<int> AllIndexesOf(this string str, string value) 
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            for (int index = 0;; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    break;
                yield return index;
            }
        }
        public static string owo(this string str)
        {
            StringBuilder sb = new StringBuilder(str);
            var rand = new Random();
            sb.Replace('r', 'w');
            sb.Replace('R', 'W');
            sb.Replace('l', 'w');
            sb.Replace('L', 'W');
            sb.Replace("you", "uu");
            sb.Replace("You","UU");
            sb.Replace("ove","uv");
            sb.Append(suffixes[rand.Next(suffixes.Length)]);

            return sb.ToString();
        }
        public static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
                return GetKCombs(list, length - 1)
            .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0), 
            (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public static void Shuffle<T>(this IList<T> list,Random rng)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
       
}
