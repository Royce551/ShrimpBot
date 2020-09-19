using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shrimpbot.Configuration;

namespace Shrimpbot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;
        private readonly ConfigurationFile config;

        public CommandHandler(DiscordSocketClient client, CommandService commands, ConfigurationFile config)
        {
            this.commands = commands;
            this.client = client;
            this.config = config;

            services = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(client)
                .BuildServiceProvider();
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;

            var argPos = 0;

            if (!(message.HasStringPrefix("s#", ref argPos)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(client, message);

            var result = await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: services);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync($"ok {result.ErrorReason}");
        }
    }
}
