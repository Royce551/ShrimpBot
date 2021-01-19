using Discord;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;

namespace Shrimpbot.Services
{
    public class LoggingService
    {
        // TODO: dunno what to do with this
        public static void LogToTerminal(LogSeverity severity, string message)
        {
            Console.ForegroundColor = severity switch
            {
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
            Console.WriteLine($"[{severity} {DateTime.Now:T}] {message}");
            Console.ResetColor();
        }

    }
}
