using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Utility")]
    [Summary("Some useful commands")]
    public class UtilityModule : InteractiveBase
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }
        public DatabaseManager Database { get; set; }

        [Command("uinfo")]
        [Summary("Gets information about a user.")]
        public async Task UserInfo(IUser runner = null)
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
        public async Task UserPicture(IUser person = null)
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            string imagePath;
            if (person is null) imagePath = Context.User.GetAvatarUrl(size: 1024); else imagePath = person.GetAvatarUrl(size: 1024);
            embedBuilder.WithImageUrl(imagePath);
            await ReplyAsync(embed: embedBuilder.Build());
        }
        [Command("timer", RunMode = RunMode.Async)]
        [Summary("Creates a short timer")]
        [Remarks("`length` - Length\n`unit` - A unit of time, can be seconds, minutes, hours, or days\n*`message` - An optional message")]
        public async Task Timer(float length, string unit, string message = null)
        {
            var timerLength = unit switch
            {
                "second" or "seconds" => TimeSpan.FromSeconds(length),
                "minute" or "minutes" => TimeSpan.FromMinutes(length),
                "hour" or "hours" => TimeSpan.FromHours(length),
                "day" or "days" => TimeSpan.FromDays(length),
                _ => TimeSpan.FromSeconds(length)
            };
            if (timerLength.Ticks <= 0)
            {
                await ReplyAsync($"Timer has to run for longer than 0 seconds");
                return;
            }

            var sendInChannel = PermissionsUtils.CheckForPermissions(Context, GuildPermission.ManageMessages);
            if (!sendInChannel)
                await ReplyAsync($"Timer has been set for {timerLength.TotalSeconds} seconds! Since you don't seem to be a moderator, I will send the message in DMs.");
            else await ReplyAsync($"Timer has been set for {timerLength.TotalSeconds} seconds!");
            if (timerLength.TotalHours > 1) await ReplyAsync($"If {Config.Name} restarts while the timer is running, the timer will be lost.");

            await Task.Delay(timerLength);

            var embed = MessagingUtils.GetShrimpbotEmbedBuilder();
            embed.Title = ":alarm_clock: Timer Elapsed";
            embed.Description = $"{Context.User.Mention}, your timer has elapsed.";
            if (message != null) embed.AddField("Message", message);

            if (!sendInChannel) await Context.User.SendMessageAsync(embed: embed.Build());
            else await ReplyAsync(embed: embed.Build());
        }
    }
}
