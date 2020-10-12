using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
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

            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            embedBuilder.WithAuthor(user);
            embedBuilder.WithDescription(user.Status.ToString());
            embedBuilder.AddField("General", 
                $"**User ID**: {user.Id}\n" +
                $"**Created**: {user.CreatedAt}\n" +
                $"**Human?**: {!user.IsBot}");
            embedBuilder.AddField("ShrimpBot",
                $"**Money**: {Config.CurrencySymbol}{databaseUser.Money}\n" +
                $"**Cuteness**: {databaseUser.Cuteness}\n" +
                $"**Bot Permission Level**: {databaseUser.BotPermissions}\n");
            await ReplyAsync(embed:embedBuilder.Build());
        }
    }
}
