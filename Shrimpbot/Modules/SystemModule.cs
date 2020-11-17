using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("System")]
    [Summary("Basic interaction with Shrimpbot")]
    public class SystemModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }
        public BotRuntimeInformation RuntimeInformation { get; set; }

        [Command("ping")]
        [Summary("Pings Shrimpbot")]
        public async Task Ping()
        {
            var watch = new Stopwatch();
            watch.Start();
            var message = await ReplyAsync("pinging");
            watch.Stop();
            await message.ModifyAsync(msg => msg.Content =
            $"Pong! I'm here!\n" +
            $"Message latency: {watch.ElapsedMilliseconds}ms\n" +
            $"Gateway: {Client.Latency}ms");
        }
        [Command("help")]
        [Summary("Gets a list of commands")]
        public async Task Help(string search = null)
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            if (search is null)
            {
                List<ModuleInfo> modules = CommandService.Modules.OrderBy(x => x.Name).ToList();
                foreach (ModuleInfo module in modules)
                {
                    if (module.Name == "Bot Management") continue;
                    string summary = $"{module.Summary}\r\n\r\n";

                    summary += string.Join(", ", module.Commands.Select(x => x.Name));
                    embedBuilder.AddField(module.Name, summary, inline: true);
                }
                await ReplyAsync(
                    ":information_source: **Shrimpbot Help**\n" +
                    $"To get more information about a category, type {Config.Prefix}help [category].", false, embedBuilder.Build());
            }
            else
            {
                ModuleInfo module = CommandService.Modules.FirstOrDefault(x => x.Name.ToLower().StartsWith(search.ToLower()));
                if (module is null)
                {
                    await ReplyAsync($"{MessagingUtils.InvalidParameterEmote} The category you tried to search for doesn't seem to exist.");
                    return;
                }
                string summary = $"{module.Summary}\r\n\r\n";
                foreach (CommandInfo command in module.Commands)
                {
                    summary += $"__{command.Name}__ - {command.Summary ?? "No description"}\n";
                }
                embedBuilder.AddField(module.Name, summary, inline: true);
                await ReplyAsync(":information_source: **Shrimpbot Help**", embed: embedBuilder.Build());
            }
        }
        [Command("about")]
        [Summary("Gets information about Shrimpbot")]
        public async Task About()
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            embedBuilder.ImageUrl = "https://cdn.discordapp.com/attachments/556283742008901645/600468110109048837/Banner.png";
            embedBuilder.AddField($"{Config.Name} {Config.Version}", "by Squid Grill (and open source contributors)");
            embedBuilder.AddField("Hosting",
                $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n" +
                $"Discord.NET {DiscordConfig.Version}\n" +
                $"{Environment.OSVersion.VersionString}"
                );
            embedBuilder.AddField("Links",
                "Official ShrimpBot Discord Server: https://discord.gg/fuJ6J4s\n" +
                "GitHub Repository: https://github.com/Royce551/ShrimpBot");
            embedBuilder.WithFooter("Thank you for using ShrimpBot! ❤");
            await Client.CurrentUser.ModifyAsync(x => x.Username = Config.Name);
            await ReplyAsync(null, false, embedBuilder.Build());
        }
        [Command("uptime")]
        [Summary("Shows how long Shrimpbot has been up")]
        public async Task Uptime()
        {
            var uptime = DateTime.Now - RuntimeInformation.StartupTime;
            await ReplyAsync($":clock10: {Config.Name} has been up for {uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes, and {uptime.Seconds} seconds!");
        }
        [Command("stats")]
        [Summary("Shows some interesting information about Shrimpbot")]
        public async Task Stats()
        {
            var uptime = DateTime.Now - RuntimeInformation.StartupTime;
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            embedBuilder.AddField($"{Config.Name} stats",
                $"Uptime: {uptime}\n" +
                $"Commands handled: {RuntimeInformation.CommandsHandled}");
            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}
