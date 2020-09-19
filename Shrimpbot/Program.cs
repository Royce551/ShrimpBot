using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Shrimpbot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new DiscordSocketClient();
            var config = ConfigurationManager.Read();
            var database = new LiteDatabase(Path.Combine(DatabaseManager.DatabasePath, "database.sdb1"));
            client.Log += Client_Log;
            var commandHandler = new CommandHandler(client, new CommandService(), config, database);
            await commandHandler.InstallCommandsAsync();

            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            while (true) Console.ReadLine();
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
