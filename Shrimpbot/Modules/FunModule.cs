using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using System;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using System.Net;

namespace Shrimpbot.Modules
{
    [Name("Fun")]
    [Summary("fun n' games n' stuff")]
    public class FunModule : InteractiveBase
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
        [Command("guess", RunMode = RunMode.Async)]
        [Summary("Guess the number!")]
        public async Task Guess(int maxNumber = 100)
        {
            var runner = DatabaseManager.GetUser(Db, Context.User.Id);
            int number = new Random().Next(1, maxNumber);
            int remainingAttempts = (int)Math.Floor(Math.Log(maxNumber, 2)) - 1;
            bool correctGuess = false;
            await ReplyAsync($"What's my number? 1-{maxNumber}. You have {remainingAttempts + 1} attempts. Say 'quit' to quit. Good luck!");
            while (!correctGuess)
            {
                var response = await NextMessageAsync();
                if (response.Author.Id != Context.User.Id) break;
                if (response.Content == "quit") return;
                if (response.Content == number.ToString())
                {
                    if (maxNumber >= 100)
                    {
                        await ReplyAsync($":tada: You were correct! Great job! You got 50 {Config.Currency} for your performance.");
                        runner.Money += 50;
                        DatabaseManager.WriteUser(Db, runner);
                    }
                    else await ReplyAsync($":tada: You were correct! Great job! You didn't get any {Config.Currency} because your max number was lower than usual..");
                }
                if (remainingAttempts == 0)
                {
                    await ReplyAsync($":boom: Oh noo! You lost. The number was {number}.");
                    return;
                }
                if (int.TryParse(response.Content, out int responseNumber))
                {
                    if (number == responseNumber) break;
                    else if (number > responseNumber) await ReplyAsync($":x: Incorrect. The number is greater than your guess. You have {remainingAttempts} attempt(s) remaining!");
                    else await ReplyAsync($":x: Incorrect. The number is less than your guess. You have {remainingAttempts} attempt(s) remaining!");
                }
                else await ReplyAsync("Invalid guess. If you need to quit, type 'quit'.");
                remainingAttempts--;
            }
        }
    }
}
