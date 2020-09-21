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
        public ConfigurationFile Config { get; set; }
        public LiteDatabase Db { get; set; }
        [Command("balance")]
        [Summary("Gets your current balance")]
        public async Task Balance()
        {
            await ReplyAsync($"You have {DatabaseManager.GetUser(Db, Context.User.Id).Money} {Config.Currency}.");
        }
        [Command("pay")]
        [Summary("Gives someone some money")]
        public async Task Pay(IUser user, int amount)
        {
            var runner = DatabaseManager.GetUser(Db, Context.User.Id);
            var reciever = DatabaseManager.GetUser(Db, user.Id);
            runner.Money -= amount;
            reciever.Money += amount;
            await ReplyAsync($"Gave {Config.CurrencySymbol}{amount} to {user.Username}, leaving you with {Config.CurrencySymbol}{runner.Money}.");
            DatabaseManager.WriteUser(Db, runner);
            DatabaseManager.WriteUser(Db, reciever);
        }
    }
}
