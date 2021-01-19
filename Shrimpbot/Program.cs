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
                        database.Database.Dispose();
                        LoggingService.LogToTerminal(LogSeverity.Info, $"{config.Name} is shutting down.");
                        return;
                    case "setbotadmin":
                        while (true)
                        {
                            Console.WriteLine("Type the user ID of the user you want to set as admin.");
                            if (ulong.TryParse(Console.ReadLine(), out var result))
                            {
                                var user = database.GetUser(result);
                                user.BotPermissions = BotPermissionLevel.BotAdministrator;
                                database.WriteUser(user);
                                Console.WriteLine("Permissions set!");
                                break;
                            }
                            else Console.WriteLine("idoit");
                        }
                        break;
                    case "setbotowner":
                        while (true)
                        {
                            Console.WriteLine("Type the user ID of the user you want to set as owner.");
                            if (ulong.TryParse(Console.ReadLine(), out var result))
                            {
                                var user = database.GetUser(result);
                                user.BotPermissions = BotPermissionLevel.Owner;
                                database.WriteUser(user);
                                Console.WriteLine("Permissions set!");
                                break;
                            }
                            else Console.WriteLine("idoit");
                        }
                        break;
                    case "stats":
                        Console.WriteLine($"Commands handled: {runtimeInformation.CommandsHandled}, Uptime - {DateTime.Now - runtimeInformation.StartupTime}");
                        break;
                    default:
                        Console.WriteLine("That's not a valid command. Valid commands are quit, setbotadmin, setbotowner, stats.");
                        break;
                }
            }
        }

        static Task Client_Log(LogMessage msg)
        {
            LoggingService.LogToTerminal(msg.Severity, msg.Message);
            return Task.CompletedTask;
        }
    }
    public class BotRuntimeInformation
    {
        public DateTime StartupTime { get; set; }
        public int CommandsHandled { get; set; }
    }
}
