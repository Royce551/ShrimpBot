using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
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
            var database = new DatabaseManager();
            var runtimeInformation = new BotRuntimeInformation { StartupTime = DateTime.Now };
            client.Log += Client_Log;
            var commandHandler = new CommandHandler(client, new CommandService(), config, database, runtimeInformation);
            await commandHandler.InstallCommandsAsync();
            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            while (true)
            {
                var response = Console.ReadLine();
                switch (response)
                {
                    case "quit":
                        
                        break;
                }
            }
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
