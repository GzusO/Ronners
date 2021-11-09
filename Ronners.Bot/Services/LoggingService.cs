using System;
using System.Threading.Tasks;
using Discord;

namespace Ronners.Bot.Services
{
    public static class LoggingService
    {
        public static async Task LogAsync(string src, LogSeverity severity, string message, Exception exception = null)
        {
            if(severity.Equals(null))
            {
                severity = LogSeverity.Warning;
            }

            await Log($" {System.DateTime.Now} ",ConsoleColor.DarkBlue);
            await Log($" {GetSeverityString(severity)} ",GetSeverityColor(severity));
            await Log($" [{GetSourceString(src)}] ",ConsoleColor.DarkGray);

            if(!string.IsNullOrEmpty(message))
                await Log($"{message}\n",ConsoleColor.White);
            else if(exception.Message is null)
                await Log($"Unknown \n{exception.StackTrace}\n",ConsoleColor.DarkRed);
            else
                await Log($"{exception.Message}\n{exception.StackTrace}\n",GetSeverityColor(severity));

        }

        private static ConsoleColor GetSeverityColor(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical    => ConsoleColor.Red,
                LogSeverity.Error       => ConsoleColor.DarkRed,
                LogSeverity.Warning     => ConsoleColor.Yellow,
                LogSeverity.Info        => ConsoleColor.Green,
                LogSeverity.Verbose     => ConsoleColor.DarkCyan,
                LogSeverity.Debug       => ConsoleColor.Magenta,
                _                       => ConsoleColor.White
            };
        }

        //Four character abbreviated strings for nicely aligned logging
        private static string GetSeverityString(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical    => "CRIT",
                LogSeverity.Error       => "EROR",
                LogSeverity.Warning     => "WARN",
                LogSeverity.Info        => "INFO",
                LogSeverity.Verbose     => "VERB",
                LogSeverity.Debug       => "DBUG",
                _                       => "UNKN"
            };
        }

        //Five character abbreviated strings for nicely aligned logging
        private static string GetSourceString(string source)
        {
            return source.ToLower() switch
            {
                "discord"   => "DISCD",
                "gateway"   => "GTWAY",
                "bot"       => "RONRS",
                "audio"     => "AUDIO",
                "ronstock"  => "MARKT",
                _           => source
            };
        }

        private static async Task Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor =color;
                Console.Write(message);
        }
    }
}