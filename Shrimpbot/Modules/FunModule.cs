using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using System;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Fun")]
    [Summary("fun n' games n' stuff")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }
        public LiteDatabase Db { get; set; }
        [Command("8ball")]
        [Summary("It's an 8 ball...")]
        public async Task EightBall(string m = null)
        {
            await ReplyAsync($":8ball: **The 8-Ball has spoken!**\nIt says - **{FunService.GetEightBall()}**");
        }
        [Command("ratecuteness")]
        [Summary("100% accurate cuteness rater.")]
        public async Task Cuteness(IUser runner = null)
        {
            DatabaseUser user;
            if (runner is null) user = DatabaseManager.GetUser(Db, Context.User.Id); else user = DatabaseManager.GetUser(Db, runner.Id);
            if (user.Cuteness == -1)
            {
                user.Cuteness = new Random().Next(1, 100);
                DatabaseManager.WriteUser(Db, user);
            }
            if (runner is null) await ReplyAsync($":two_hearts: Your cuteness level is {user.Cuteness}/100.");
            else await ReplyAsync($":two_hearts: {runner.Username}'s cuteness is {user.Cuteness}/100.");
        }
    }
}
