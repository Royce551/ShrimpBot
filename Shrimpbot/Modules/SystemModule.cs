using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    public class SystemModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile ConfigurationFile { get; set; }

        [Command("ping")]
        [Summary("Pings Shrimpbot")]
        public async Task Ping()
        {
            var watch = new Stopwatch();
            watch.Start();
            var message = await ReplyAsync("pinging");
            watch.Stop();
            await message.ModifyAsync(msg => msg.Content = $"Ping took {watch.ElapsedMilliseconds}ms!");
        }
        [Command("help")]
        [Summary("Provides a list of commands")]
        public async Task Help()
        {
            List<CommandInfo> commands = CommandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }
        [Command("about")]
        [Summary("Gets information about Shrimpbot")]
        public async Task About()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.ImageUrl = "https://cdn.discordapp.com/attachments/556283742008901645/600468110109048837/Banner.png";
            embedBuilder.AddField($"{ConfigurationFile.Name} {ConfigurationFile.Version}", "by Squid Grill");
            embedBuilder.AddField("Official Shrimpbot Discord Server", "https://discord.gg/XztEQAh");
            embedBuilder.Color = ConfigurationFile.AccentColor;
            await Client.CurrentUser.ModifyAsync(x => x.Username = $"{ConfigurationFile.Name} {ConfigurationFile.Version}");
            await ReplyAsync(null, false, embedBuilder.Build());
        }
    }
}
