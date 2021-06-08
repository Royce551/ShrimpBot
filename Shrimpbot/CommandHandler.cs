using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Shrimpbot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly DatabaseManager database;
        private readonly IServiceProvider services;
        private readonly ConfigurationFile config;
        private readonly BotRuntimeInformation runtimeInformation;

        public CommandHandler(DiscordSocketClient client, CommandService commands, ConfigurationFile config, DatabaseManager database, BotRuntimeInformation runtimeInformation)
        {
            this.commands = commands;
            this.database = database;
            this.client = client;
            this.config = config;
            this.runtimeInformation = runtimeInformation;

            client.UserJoined += Client_UserJoined;

            services = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(client)
                .AddSingleton(runtimeInformation)
                .AddSingleton(database)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }

        private Task Client_UserJoined(SocketGuildUser arg)
        {
            var server = arg.Guild;
            var databaseServer = database.GetServer(server.Id);
            try
            {
                if (arg.Username.ToLower().Contains("h0nde"))
                {
                    arg.BanAsync(reason: "Auto banned because your name was similar to a spam botter, please contact Squid Grill#6238 if this is wrong.");
                    (server.GetChannel(databaseServer.LoggingChannel) as SocketTextChannel)?.SendMessageAsync("I just banned a potential spambotter, check audit logs for details");
                }
            }
            catch
            {
                // ignored
            }
            return Task.CompletedTask;
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message) return;

            var argPos = 0;

            if (!(message.HasStringPrefix(config.Prefix, ref argPos)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(client, message);

            var result = await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: services);
            LoggingService.LogToTerminal(LogSeverity.Verbose, $"Executed a command! {context.Guild?.Name ?? "in DM's"} at {context.Channel?.Name}.");
            runtimeInformation.CommandsHandled++;
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"Oopsies! {result.ErrorReason}");
        }
    }
}
