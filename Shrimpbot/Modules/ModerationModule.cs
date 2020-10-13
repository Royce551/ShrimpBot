using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Services.Moderation;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Moderation")]
    [Summary("Commands for managing your server.")]
    public class ModerationModule : InteractiveBase
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }

        [Command("manageserver", RunMode = RunMode.Async), RequireUserPermission(GuildPermission.ManageGuild, ErrorMessage = "You don't have permission to run this command.")]
        [Summary("Configures a server.")]
        public async Task ManageServer()
        {
            var server = DatabaseManager.GetServer(Context.Guild.Id);
            var navigator = ModerationService.CreateBotModerationNavigator(server);

            async void ShowInitialPrompt()
            {
                var initialPromptEmbedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
                initialPromptEmbedBuilder.WithAuthor(Context.User);
                initialPromptEmbedBuilder.WithDescription($"{Context.Guild.Name} Settings");
                initialPromptEmbedBuilder.AddField("The following properties are available to edit:",
                    $"**1** - Allow potential NSFW in non- NSFW channels - {server.AllowsPotentialNSFW}\n" +
                    $"**2** - Logging Channel - {server.LoggingChannel}\n" +
                    $"**3** - System Channel - {server.SystemChannel}\n");
                initialPromptEmbedBuilder.WithFooter("Say 'quit' to exit.");
                await ReplyAsync(embed: initialPromptEmbedBuilder.Build());
            }
            ShowInitialPrompt();
            while (true)
            {
                var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 0, 0, -1)); // Infinite timeout
                string responseString = response.ToString().ToLower();
                if (responseString == "1") navigator.Navigate(Property.AllowsPotentialNSFW);
                else if (responseString == "2") navigator.Navigate(Property.LoggingChannel);
                else if (responseString == "3") navigator.Navigate(Property.SystemChannel);
                else if (responseString == "quit") return;
                else
                {
                    await ReplyAsync("That's not a valid property. If you need to exit, type 'quit'.");
                    continue;
                }
                await ReplyAsync(embed: navigator.GetFormattedCurrentPage(Context.User).Build());
                while (true)
                {
                    var pageResponse = await NextMessageAsync(timeout: new TimeSpan(0, 0, 0, 0, -1)); // Infinite timeout
                    string pageResponseString = pageResponse.ToString().ToLower();
                    if (pageResponseString == "quit") return;
                    if (pageResponseString == "back")
                    {
                        ShowInitialPrompt();
                        break;
                    }
                    if (navigator.Set(pageResponseString.TrimStart("set".ToCharArray()).TrimStart()))
                    {
                        DatabaseManager.WriteServer(navigator.Server);
                        ShowInitialPrompt();
                        break;
                    }
                    else
                    {
                        await ReplyAsync("Your input wasn't valid.");
                        continue;
                    }
                }
            }
        }
    }
}
