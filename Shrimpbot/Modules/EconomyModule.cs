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
        [Command("balance")]
        [Summary("Gets your current balance")]
        public async Task Balance()
        {
            await ReplyAsync($"You have {DatabaseManager.GetUser(Context.User.Id).Money} {Config.Currency}.");
        }
        [Command("pay")]
        [Summary("Gives someone some money")]
        public async Task Pay(IUser user, double amount)
        {
            if (amount <= 0)
            {
                await ReplyAsync("You can't pay someone a negative amount.");
                return;
            }
            var runner = DatabaseManager.GetUser(Context.User.Id);
            var reciever = DatabaseManager.GetUser(user.Id);
            runner.Money -= amount;
            reciever.Money += amount;
            await ReplyAsync($"Gave {Config.CurrencySymbol}{amount} to {user.Username}, leaving you with {Config.CurrencySymbol}{runner.Money}.");
            DatabaseManager.WriteUser(runner);
            DatabaseManager.WriteUser(reciever);
        }
        [Command("gamble")]
        [Summary("Throws your money away in the hopes of doubling your wealth")]
        public async Task Gamble(int bet)
        {
            var runner = DatabaseManager.GetUser(Context.User.Id);
            var rng = new Random();
            var runnergamble = rng.Next(1, 20);
            var shrimpgamble = rng.Next(1, 20);
            if (bet > runner.Money)
            {
                await ReplyAsync("Hold up! You don't even have enough money to bet this amount!");
                return;
            }
            if (bet <= 0)
            {
                await ReplyAsync("You can't bet a negative amount.");
                return;
            }
            if (runnergamble > shrimpgamble)
            {
                bet *= 3;
                runner.Money += bet;
                await ReplyAsync($"You - {runnergamble} | {Config.Name} - {shrimpgamble}, You won! Your bet got doubled, leaving you with {runner.Money} {Config.Currency}. Naisuu!!");
            }
            else
            {
                runner.Money -= bet;
                await ReplyAsync($"You - {runnergamble} | {Config.Name} - {shrimpgamble}, Damn, you lost. You now have {runner.Money} {Config.Currency}.");
            }
            DatabaseManager.WriteUser(runner);
        }
        [Command("work")]
        [Summary("Work for Squid Grill to get some money.")]
        public async Task Daily()
        {
            var runner = DatabaseManager.GetUser(Context.User.Id);
            if (DateTime.Now - runner.DailyLastClaimed >= new TimeSpan(1,0,0,0))
            {
                var rng = new Random().Next(1,3);
                string response = rng switch
                {
                    1 => "You helped Squid fix bugs in FMP and got {0} {1}.",
                    2 => "You beat theBeat out of the water and got {0} {1} for your hard work.",
                    _ => "The dev did a fucky wucky."
                };
                var moneygained = Math.Round(50 * runner.DailyBonus);
                runner.Money += moneygained;
                runner.DailyBonus += 0.01;
                runner.DailyLastClaimed = DateTime.Now;
                await ReplyAsync(string.Format(response, moneygained, Config.Currency));
            }
            else
            {
                await ReplyAsync($"You already worked for Squid today. Try again in {Math.Round(((runner.DailyLastClaimed + new TimeSpan(1,0,0,0)) - DateTime.Now).TotalHours)} hours.");
                return;
            }
            DatabaseManager.WriteUser(runner);
        }
    }
}
