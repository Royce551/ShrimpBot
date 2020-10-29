using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Information")]
    [Summary("Commands for getting information about users and servers")]
    public class InformationModule : InteractiveBase
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }

        [Command("uinfo")]
        [Summary("Gets information about a user.")]
        public async Task UserInfo(IUser runner = null)
        {
            DatabaseUser databaseUser;
            if (runner is null) databaseUser = DatabaseManager.GetUser(Context.User.Id); else databaseUser = DatabaseManager.GetUser(runner.Id);
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
            await ReplyAsync(embed:embedBuilder.Build());
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
        public async Task UserPicture(IUser person = null)
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            string imagePath;
            if (person is null) imagePath = Context.User.GetAvatarUrl(size: 1024); else imagePath = person.GetAvatarUrl(size: 1024);
            embedBuilder.WithImageUrl(imagePath);
            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}
