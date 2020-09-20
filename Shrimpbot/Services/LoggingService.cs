using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shrimpbot.Services
{
    public class LoggingService
    {
        public static void Log(LogSeverity severity, string message)
        {
            Console.ForegroundColor = severity switch
            {
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
            Console.WriteLine($"[{severity} {DateTime.Now:T}] {message}");
        }
    }
}
