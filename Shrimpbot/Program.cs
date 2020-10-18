using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using System;
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
            var commandHandler = new CommandHandler(client, new CommandService(), config);
            await commandHandler.InstallCommandsAsync();

            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            while (true) Console.ReadLine();
        }

        static Task Client_Log(LogMessage msg)
        {
            LoggingService.Log(msg.Severity, msg.Message);
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
