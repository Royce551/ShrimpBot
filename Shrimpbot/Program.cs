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
            var runtimeInformation = new BotRuntimeInformation { StartupTime = DateTime.Now };
            client.Log += Client_Log;
            var commandHandler = new CommandHandler(client, new CommandService(), config, runtimeInformation);
            await commandHandler.InstallCommandsAsync();
            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            while (true) Console.ReadLine();
        }

        static Task Client_Log(LogMessage msg)
        {
            LoggingService.Log(msg.Severity, msg.Message);
            return Task.CompletedTask;
        }
    }
    public class BotRuntimeInformation
    {
        public DateTime StartupTime { get; set; }
        public int CommandsHandled { get; set; }
    }
}
