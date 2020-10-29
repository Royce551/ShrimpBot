using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
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
            var navigator = ManagementService.CreateBotModerationNavigator(server);

            await ReplyAsync(embed: navigator.GetHomePage(Context).Build());
            while (true)
            {
                var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 0, 0, -1)); // Infinite timeout
                string responseString = response.ToString().ToLower();
                if (responseString == "1") navigator.Navigate(ServerProperty.AllowsPotentialNSFW);
                else if (responseString == "2") navigator.Navigate(ServerProperty.LoggingChannel);
                else if (responseString == "3") navigator.Navigate(ServerProperty.SystemChannel);
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
                        await ReplyAsync(embed: navigator.GetHomePage(Context).Build());
                        break;
                    }
                    if (navigator.Set(pageResponseString.TrimStart("set".ToCharArray()).TrimStart()))
                    {
                        DatabaseManager.WriteServer(navigator.Server);
                        await ReplyAsync(embed: navigator.GetHomePage(Context).Build());
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
        [Command("usersettings", RunMode = RunMode.Async)]
        [Summary("Configures settings for yourself.")]
        public async Task UserSettings()
        {
            var user = DatabaseManager.GetUser(Context.User.Id);
            var navigator = ManagementService.CreateUserSettingsNavigator(user);

            await ReplyAsync(embed: navigator.GetHomePage(Context).Build());
            while (true)
            {
                var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 0, 0, -1)); // Infinite timeout
                string responseString = response.ToString().ToLower();
                if (responseString == "1") navigator.Navigate(UserProperty.TimeZone);
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
                        await ReplyAsync(embed: navigator.GetHomePage(Context).Build());
                        break;
                    }
                    if (navigator.Set(pageResponseString.TrimStart("set".ToCharArray()).TrimStart()))
                    {
                        DatabaseManager.WriteUser(navigator.User);
                        await ReplyAsync(embed: navigator.GetHomePage(Context).Build());
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
