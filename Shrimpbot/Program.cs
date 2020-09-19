using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Shrimpbot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new DiscordSocketClient();
            var config = ConfigurationManager.Read();
            client.Log += Client_Log;
            client.Ready += Client_Ready;

            var commandHandler = new CommandHandler(client, new CommandService(), config);
            await commandHandler.InstallCommandsAsync();

            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            while (true) Console.ReadLine();
        }

        private static Task Client_Ready()
        {
            throw new NotImplementedException();
        }

        static Task Client_Log(LogMessage msg)
        {
            Console.ForegroundColor = msg.Severity switch
            {
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
            Console.WriteLine($"[{msg.Severity} {DateTime.Now:T}] {msg.Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
