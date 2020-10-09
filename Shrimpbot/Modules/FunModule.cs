using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Shrimpbot.Utilities;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using System;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using System.Net;
using System.IO;

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
        [Command("8ball", ignoreExtraArgs:true)]
        [Summary("It's an 8 ball...")]
        public async Task EightBall()
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
            try
            {
                if (maxNumber <= 0)
                {
                    await ReplyAsync("That's not a valid max number!");
                    return;
                }
                var runner = DatabaseManager.GetUser(Db, Context.User.Id);
                int number = new Random().Next(1, maxNumber + 1);
                int maxguesses = (int)Math.Floor(Math.Log(maxNumber, 2));
                int remainingAttempts = maxguesses - 1;
                bool correctGuess = false;
                await ReplyAsync($"What's my number? 1-{maxNumber}. You have {remainingAttempts + 1} attempts. Say 'quit' to quit. Good luck!");
                while (!correctGuess)
                {
                    var response = await NextMessageAsync();
                    if (response.Author.Id != Context.User.Id || response is null) continue;
                    if (response.Content == "quit")
                    {
                        await ReplyAsync($"Seeya! (The number was {number} by the way)");
                        return;
                    }
                    if (response.Content == number.ToString()) break;
                    if (remainingAttempts <= 0)
                    {
                        await ReplyAsync($":boom: Oh noo! You lost. The number was {number}.");
                        return;
                    }
                    if (int.TryParse(response.Content, out int responseNumber))
                    {
                        if (number == responseNumber) break;
                        else if (number > responseNumber) await ReplyAsync($":x: Incorrect. The number is greater than your guess. You have {remainingAttempts} attempt(s) remaining!");
                        else await ReplyAsync($":x: Incorrect. The number is less than your guess. You have {remainingAttempts} attempt(s) remaining!");
                        remainingAttempts--;
                    }
                    else await ReplyAsync("Invalid guess. If you need to quit, type 'quit'.");
                }
                double payMultiplier = maxNumber / Math.Pow(2, maxguesses);
                await ReplyAsync($":tada: You were correct! Great job! You got {Math.Round(maxguesses * 10 * payMultiplier)} {Config.Currency} for your performance.");
                runner.Money += Math.Round(maxguesses * 10 * payMultiplier);
                DatabaseManager.WriteUser(Db, runner);
            }
            catch (Exception e)
            {
                LoggingService.Log(LogSeverity.Critical, e.Message + "\n" + e.StackTrace);
                await ReplyAsync("Fucky wucky" + e.Message + "\n" + e.StackTrace);
            }
        }
        [Command("cute")]
        [Summary("Gets a random cute image")]
        public async Task Cute(string type = "all", string source = "legacy")
        {
            var imageType = type.ToLower() switch
            {
                "anime" => ImageType.Anime,
                "catgirls" => ImageType.Catgirls,
                "all" => ImageType.All,
                _ => ImageType.All,
            };
            var imageSource = source.ToLower() switch
            {
                "local" => ImageSource.LocalImages,
                "legacy" => ImageSource.LegacyImages,
                "online" => ImageSource.Online,
                _ => ImageSource.LocalImages
            };
            var image = CuteService.GetImage(imageSource, imageType);

            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            embedBuilder.ImageUrl = $"attachment://image.png";
            embedBuilder.WithFooter($"Creator: {image.Creator}\nSource: {image.Source}");
            var embed = embedBuilder.Build();
            await Context.Channel.SendFileAsync(new FileStream(image.Path, FileMode.Open), "image.png", embed: embed);
        }
    }
}
