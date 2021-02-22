using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Shrimpbot.Modules
{
    [Name("Utility")]
    [Summary("Some useful commands")]
    public class UtilityModule : InteractiveBase
    {
        public DiscordSocketClient Client { get; set; } = default!;
        public CommandService CommandService { get; set; } = default!;
        public ConfigurationFile Config { get; set; } = default!;
        public DatabaseManager Database { get; set; } = default!;
        public BotRuntimeInformation RuntimeInformation { get; set; } = default!;

        [Command("uinfo")]
        [Summary("Gets information about a user.")]
        public async Task UserInfo(IUser? runner = null)
        {
            DatabaseUser databaseUser;
            if (runner is null) databaseUser = Database.GetUser(Context.User.Id); else databaseUser = Database.GetUser(runner.Id);
            var user = Client.GetUser(databaseUser.Id);
            var guildUser = Context.Guild?.GetUser(databaseUser.Id);

            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            embedBuilder.WithAuthor(user);
            embedBuilder.WithDescription(user.Status.ToString());

            embedBuilder.AddField("General",
                $"**User ID**: {user.Id}\n" +
                $"**Created**: {user.CreatedAt}\n" +
                $"**Human?**: {!user.IsBot}");

            if (guildUser != null) embedBuilder.AddField("This Server",
                $"**Nickname**: {guildUser.Nickname}\n" +
                $"**Joined**: {guildUser.JoinedAt}\n");

            embedBuilder.AddField("ShrimpBot",
                $"**Money**: {Config.CurrencySymbol}{databaseUser.Money}\n" +
                $"**Cuteness**: {databaseUser.Cuteness}\n" +
                $"**Bot Permission Level**: {databaseUser.BotPermissions}\n");

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("sinfo")]
        [Summary("Gets information about a server.")]
        public async Task ServerInfo()
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            var server = Context.Guild;

            if (server is null)
            {
                await ReplyAsync("Looks like we aren't in a server.");
                return;
            }

            embedBuilder.WithAuthor(server.Name, server.IconUrl);
            embedBuilder.AddField("General",
                $"**Created**: {server.CreatedAt}\n" +
                $"**Owner**: {server.Owner}\n" +
                $"**Members**: {server.MemberCount}\n");

            embedBuilder.AddField("Misc. Information",
                $"**Text Channels**: {server.TextChannels.Count}\n" +
                $"**Roles**: {server.Roles.Count}\n" +
                $"**Emotes**: {server.Emotes.Count}\n" +
                $"**Content Filter**: {server.ExplicitContentFilter}\n" +
                $"**Server Boost Level**: {server.PremiumTier}");

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("pic")]
        [Summary("Shows a user's profile picture.")]
        [Remarks("*`user` - An optional user to get the profile picture for. If not specified, you.")]
        public async Task UserPicture(IUser? person = null)
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            var imagePath = (person ?? Context.User).GetAvatarUrl(size: 1024);

            embedBuilder.WithImageUrl(imagePath);
            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("timer", RunMode = RunMode.Async)]
        [Summary("Creates a short timer")]
        [Remarks("`length` - Length\n`unit` - A unit of time, can be seconds, minutes, hours, or days\n*`message` - An optional message")]
        public async Task Timer(TimeSpan length, [Remainder] string? message = null)
        {
            if (length.Ticks <= 0)
            {
                await ReplyAsync("Timer has to run for longer than 0 seconds");
                return;
            }

            await ReplyAsync($"Timer has been set for {length.TotalSeconds} sec.");
            RuntimeInformation.Timers.CreateTimer(Context.User, message, length);
        }
        [Command("timers")]
        [Summary("Shows you the timers you currently have running")]
        public async Task Timers()
        {
            var timers = RuntimeInformation.Timers.RunningTimers.Where(x => x.CreatorID == Context.User.Id);
            if (!timers.Any())
            {
                await ReplyAsync("You don't have any running timers.");
                return;
            }

            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            embedBuilder.WithTitle("Your running timers");
            embedBuilder.WithAuthor(Context.User);
            int i = 1;
            foreach (var timer in timers)
            {
                embedBuilder.AddField($"Timer #{i}",
                    $"**Elapses**: {timer.Elapses}\n" +
                    $"**Message**: {timer.Message}", inline: true);
                i++;
            }
            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}
