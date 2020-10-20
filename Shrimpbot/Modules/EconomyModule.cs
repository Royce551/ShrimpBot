using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Economy")]
    [Summary("bling bling")]
    public class EconomyModule : ModuleBase<SocketCommandContext>
    {
        static Random rng = new Random();

        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }
        [Command("balance")]
        [Summary("Gets your current balance")]
        public async Task Balance()
        {
            await ReplyAsync($"You have {DatabaseManager.GetUser(Context.User.Id).Money} {string.Format("{0:n}", Config.Currency)}.");
        }
        [Command("pay")]
        [Summary("Gives someone some money")]
        public async Task Pay(IUser user, decimal amount)
        {
            if (amount <= 0)
            {
                await ReplyAsync("You can't pay someone a negative amount.");
                return;
            }
            var runner = DatabaseManager.GetUser(Context.User.Id);
            var reciever = DatabaseManager.GetUser(user.Id);

            if (amount > runner.Money)
            {
                await ReplyAsync("You can't give someone more money than you have.");
                return;
            }

            if (Context.User.Id == user.Id)
            {
                await ReplyAsync($"You gave {Config.CurrencySymbol}{amount} to yourself, you still have {Config.CurrencySymbol}{string.Format("{0:n}", runner.Money)} - what a surprise.");
            }
            else
            {
                runner.Money -= amount;
                reciever.Money += amount;
                await ReplyAsync($"Gave {Config.CurrencySymbol}{amount} to {user.Username}, leaving you with {Config.CurrencySymbol}{string.Format("{0:n}", runner.Money)}");
                DatabaseManager.WriteUser(runner);
                DatabaseManager.WriteUser(reciever);
            }
        }
        [Command("gamble")]
        [Summary("Throws your money away in the hopes of doubling your wealth")]
        public async Task Gamble(decimal bet)
        {
            var runner = DatabaseManager.GetUser(Context.User.Id);
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
                bet *= 2;
                runner.Money += bet;
                await ReplyAsync($"You - {runnergamble} | {Config.Name} - {shrimpgamble}, You won! Your bet got doubled, leaving you with {string.Format("{0:n}", runner.Money)} {Config.Currency}. Naisuu!!");
            }
            else
            {
                runner.Money -= bet;
                await ReplyAsync($"You - {runnergamble} | {Config.Name} - {shrimpgamble}, Damn, you lost. You now have {string.Format("{0:n}", runner.Money)} {Config.Currency}.");
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
                var responses = new string[]
                {
                    "You helped Squid fix bugs in FMP and got {0} {1}.",
                    "You beat theBeat out of the water and got {0} {1} for your hard work.",
                    "The dev did a fucky wucky."
                };

                string response = responses[rng.Next(0, responses.Length - 1)];
                decimal moneygained = (decimal)Math.Round(50 * runner.DailyBonus);
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
        [Command("leaderboard")]
        [Summary("See how your wealth stacks up to others.")]
        public async Task Leaderboard()
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            var users = DatabaseManager.GetAllUsers().GetRange(0, 10).OrderByDescending(o => o.Money);
            var stringBuilder = new StringBuilder();
            int i = 1;
            foreach (var user in users)
            {
                stringBuilder.AppendLine($"{i} - {Client.GetUser(user.Id).Username}: {Config.CurrencySymbol}{string.Format("{0:n}", user.Money)}");
                i++;
            }

            embedBuilder.AddField($"Top {Config.Name} users",
                stringBuilder.ToString());
            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}
