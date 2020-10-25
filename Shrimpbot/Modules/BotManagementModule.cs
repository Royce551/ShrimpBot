using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Bot Management")]
    [Summary("Provides commands for manually managing ShrimpBot - Intended to be used only by bot administrators.")]
    public class BotManagementModule : InteractiveBase
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }

        [Command("addimagetodatabase")]
        public async Task AddImage(string path, string type, string creator, string uploader, string source )
        {
            var runner = DatabaseManager.GetUser(Context.User.Id);
            if (runner.BotPermissions < BotPermissionLevel.BotAdministrator)
            {
                await ReplyAsync(MessagingUtils.GetNoPermissionsString());
                return;
            }
            if (creator == "null") creator = string.Empty;
            if (uploader == "null") uploader = string.Empty;
            if (source == "null") source = string.Empty;
            var imageType = type.ToLower() switch
            {
                "anime" => ImageType.Anime,
                "catgirls" => ImageType.Catgirls,
                "all" => ImageType.All,
                _ => ImageType.All,
            };
            DatabaseManager.CreateImage(path, imageType, new CuteImage { Creator = creator, Path = path, ImageSource = source, Uploader = uploader });
        }
        [Command("databaseevalsql")]
        public async Task DatabaseEvaluateSql(string sql)
        {
            var runner = DatabaseManager.GetUser(Context.User.Id);
            if (runner.BotPermissions < BotPermissionLevel.BotAdministrator)
            {
                await ReplyAsync(MessagingUtils.GetNoPermissionsString());
                return;
            }
            DatabaseManager.ExecuteSql(sql);
        }
    }
}
