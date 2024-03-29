﻿using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
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
        public DatabaseManager Database { get; set; }

        [Command("addimagetodatabase")]
        public async Task AddImage(string path, string type, string creator, string source)
        {
            var runner = Database.GetUser(Context.User.Id);
            if (runner.BotPermissions < BotPermissionLevel.BotAdministrator)
            {
                await ReplyAsync(MessagingUtils.GetNoPermissionsString());
                return;
            }
            if (creator == "null") creator = string.Empty;
            if (source == "null") source = string.Empty;
            var imageType = type.ToLower() switch
            {
                "anime" => ImageType.Anime,
                "catgirls" => ImageType.Catgirls,
                "all" => ImageType.All,
                _ => ImageType.All,
            };
            Database.CreateImage(path, imageType, new CuteImage { Creator = creator, Path = path, ImageSource = source });
        }
        [Command("databaseevalsql")]
        public async Task DatabaseEvaluateSql(string sql)
        {
            var runner = Database.GetUser(Context.User.Id);
            if (runner.BotPermissions < BotPermissionLevel.BotAdministrator)
            {
                await ReplyAsync(MessagingUtils.GetNoPermissionsString());
                return;
            }
            Database.ExecuteSql(sql);
        }
        [Command("addparagraph")]
        public async Task AddParagraph(string name, string paragraph)
        {
            var runner = Database.GetUser(Context.User.Id);
            if (runner.BotPermissions < BotPermissionLevel.BotAdministrator)
            {
                await ReplyAsync(MessagingUtils.GetNoPermissionsString());
                return;
            }
            await System.IO.File.WriteAllTextAsync($"Paragraphs/{name}.txt", paragraph);
        }
    }
}
