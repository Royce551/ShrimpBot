using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Economy")]
    [Summary("bling bling")]
    public class EconomyModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile ConfigurationFile { get; set; }
        public LiteDatabase Database { get; set; }
        [Command("balance")]
        [Summary("Gets your current balance")]
        public async Task Balance()
        {
            await ReplyAsync($"Your balance - {DatabaseManager.GetUser(Database, Context.User.Id).Money}");
        }
        [Command("takemoneydebug")]
        public async Task TakeMoneyDebug(int amount)
        {
            var user = DatabaseManager.GetUser(Database, Context.User.Id);
            user.Money -= amount;
            await ReplyAsync($"Took {amount} money from you, leaving you with {user.Money}.");
            DatabaseManager.WriteUser(Database, user);
        }
        [Command("pay")]
        [Summary("Gives someone some money")]
        public async Task Pay(IUser user, int amount)
        {
            var runner = DatabaseManager.GetUser(Database, Context.User.Id);
            var reciever = DatabaseManager.GetUser(Database, user.Id);
            runner.Money -= amount;
            reciever.Money += amount;
            await ReplyAsync($"Gave {amount} to {user.Username}, leaving you with {runner.Money}.");
            DatabaseManager.WriteUser(Database, runner);
            DatabaseManager.WriteUser(Database, reciever);
        }
    }
}
