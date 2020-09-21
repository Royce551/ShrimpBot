using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Configuration;
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

        [Command("ping")]
        [Summary("Pings Shrimpbot")]
        public async Task Ping()
        {
            var watch = new Stopwatch();
            watch.Start();
            var message = await ReplyAsync("pinging");
            watch.Stop();
            await message.ModifyAsync(msg => msg.Content = $"Pong! I'm here! That took {watch.ElapsedMilliseconds}ms.");
        }
        [Command("help")]
        [Summary("Gets a list of commands")]
        public async Task Help()
        {
            List<ModuleInfo> modules = CommandService.Modules.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (ModuleInfo module in modules)
            {
                string summary = $"{module.Summary}\r\n\r\n";
                
                foreach (CommandInfo command in module.Commands)
                {
                    // Get the command Summary attribute information
                    string embedFieldText = command.Summary ?? "No description available\n";

                    summary += $"{command.Name} - {command.Summary}\n";
                }
                embedBuilder.AddField(module.Name, summary);
            }
            embedBuilder.Color = new Color(51, 139, 193);
            await ReplyAsync(":information_source: **Shrimpbot Help**", false, embedBuilder.Build());
        }
        [Command("about")]
        [Summary("Gets information about Shrimpbot")]
        public async Task About()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.ImageUrl = "https://cdn.discordapp.com/attachments/556283742008901645/600468110109048837/Banner.png";
            embedBuilder.AddField($"{Config.Name} {Config.Version}", "by Squid Grill");
            embedBuilder.AddField("Official Shrimpbot Discord Server", "https://discord.gg/fuJ6J4s");
            embedBuilder.Color = new Color(51, 139, 193);
            await Client.CurrentUser.ModifyAsync(x => x.Username = $"{Config.Name} {Config.Version}");
            await ReplyAsync(null, false, embedBuilder.Build());
        }
    }
}
