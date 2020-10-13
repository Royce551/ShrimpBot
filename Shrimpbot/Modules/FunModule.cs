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
using System.Text;

namespace Shrimpbot.Modules
{
    [Name("Fun")]
    [Summary("fun n' games n' stuff")]
    public class FunModule : InteractiveBase
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }

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
            if (runner is null) user = DatabaseManager.GetUser(Context.User.Id); else user = DatabaseManager.GetUser(runner.Id);
            if (user.Cuteness == -1)
            {
                user.Cuteness = new Random().Next(1, 100);
                DatabaseManager.WriteUser(user);
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
                if (maxNumber <= 1) // Results in 0 max attempts
                {
                    await ReplyAsync("That's not a valid max number!");
                    return;
                }
                var runner = DatabaseManager.GetUser(Context.User.Id);
                int number = new Random().Next(1, maxNumber + 1);
                int maxguesses = (int)Math.Floor(Math.Log(maxNumber, 2)); // Ensures 1/3 chance of winning in most cases
                int remainingAttempts = maxguesses - 1;
                await ReplyAsync($"What's my number? 1-{maxNumber}. You have {remainingAttempts + 1} attempts. Say 'quit' to quit. Good luck!");
                while (true)
                {
                    var response = await NextMessageAsync(timeout:new TimeSpan(0, 0, 0, 0, -1)); // Infinite timeout
                    if (response.Author.Id != Context.User.Id || response is null) continue;
                    if (response.Content == "quit")
                    {
                        await ReplyAsync($"Seeya! (The number was {number} by the way)");
                        return;
                    }
                    if (response.Content == number.ToString()) break; // figure out why this works and why the other doesn't
                    if (int.TryParse(response.Content, out int responseNumber))
                    {
                        if (remainingAttempts <= 0)
                        {
                            await ReplyAsync($":boom: Oh noo! You lost. The number was {number}.");
                            return;
                        }
                        if (number == responseNumber) break; // Correct answer
                        else if (number > responseNumber) await ReplyAsync($":x: Incorrect. The number is greater than your guess. You have {remainingAttempts} attempt(s) remaining!");
                        else await ReplyAsync($":x: Incorrect. The number is less than your guess. You have {remainingAttempts} attempt(s) remaining!");
                        remainingAttempts--;
                    }
                    else await ReplyAsync("Invalid guess. If you need to quit, type 'quit'.");
                }
                double payMultiplier = maxNumber / Math.Pow(2, maxguesses); // Awards double for when the max number isn't a power of 2 (highest chances of winning)
                await ReplyAsync($":tada: You were correct! Great job! You got {Math.Round(maxguesses * 10 * payMultiplier)} {Config.Currency} for your performance.");
                runner.Money += Math.Round(maxguesses * 10 * payMultiplier);
                DatabaseManager.WriteUser(runner);
            }
            catch (Exception e)
            {
                LoggingService.Log(LogSeverity.Critical, e.Message + "\n" + e.StackTrace);
                await ReplyAsync("Fucky wucky" + e.Message + "\n" + e.StackTrace);
            }
        }
        [Command("battle", RunMode = RunMode.Async)]
        [Summary("Battle people")]
        public async Task Battle()
        {
            var battle = FunService.CreateBattle(Client.GetUser(Context.User.Id).Username);
            while (!battle.Enemy.IsDead())
            {
                if (battle.Protagonist.IsDead())
                {
                    await ReplyAsync(":bone: You died :(");
                    return;
                }
                await ReplyAsync(embed: battle.GetFormattedStatus(Context.User).Build());
                var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 0, 0, -1)); // Infinite timeout

                (int proDamageDealt, int proManaUsed, int enemyDamageDealt, int enemyManaUsed, string turnResponse) turnResults;
                if (response.Content == "a") turnResults = battle.DoTurn(ShrimpBattleActionType.Attack);
                else if (response.Content == "m") turnResults = battle.DoTurn(ShrimpBattleActionType.UseMagic);
                else if (response.Content == "h") turnResults = battle.DoTurn(ShrimpBattleActionType.Heal);
                else if (response.Content == "r" || response.Content == "quit")
                {
                    await ReplyAsync(":person_running: You ran away.");
                    return;
                }
                else
                {
                    await ReplyAsync(":x: That's not a valid action. If you need to quit, type 'quit'.");
                    continue;
                }
                await ReplyAsync(
                    $"**{turnResults.turnResponse}**\n\n" +
                    $"**You** deal **{turnResults.proDamageDealt} damage** and use {turnResults.proManaUsed} mana.\n" +
                    $"**{battle.Enemy.Name}** deals **{turnResults.enemyDamageDealt} damage** and uses {turnResults.enemyManaUsed} of their mana.");
            }
            await ReplyAsync($":tada: You win! You got 50 {Config.Currency} for your performance.");
            var user = DatabaseManager.GetUser(Context.User.Id);
            user.Money += 50;
            DatabaseManager.WriteUser(user);
        }
        [Command("cute")]
        [Summary("Gets a random cute image")]
        public async Task Cute(string type = "all", string source = "curated")
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
                "curated" => ImageSource.Curated,
                "legacy" => ImageSource.LegacyImages,
                "online" => ImageSource.Online,
                _ => ImageSource.Curated
            };
            var image = CuteService.GetImage(imageSource, imageType);

            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            if (imageSource == ImageSource.Online) // Involves URLs
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrEmpty(image.Creator)) builder.AppendLine($"Creator: {image.Creator}");
                if (!string.IsNullOrEmpty(image.Uploader)) builder.AppendLine($"Uploaded by {image.Uploader}");
                builder.AppendLine($"Source: {image.Source}");

                embedBuilder.ImageUrl = image.Path;
                embedBuilder.Url = image.Path;
                embedBuilder.WithDescription(builder.ToString());
                var embed = embedBuilder.Build();
                await ReplyAsync(embed: embed);
            }
            else // Involves local files
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrEmpty(image.Creator)) builder.AppendLine($"Creator: {image.Creator}");
                if (!string.IsNullOrEmpty(image.Uploader)) builder.AppendLine($"Uploaded by {image.Uploader}");
                builder.AppendLine($"Source: {image.Source}");

                string path = Path.GetFileName(image.Path);
                embedBuilder.ImageUrl = $"attachment://{path}";
                embedBuilder.WithDescription(builder.ToString());
                var embed = embedBuilder.Build();
                await Context.Channel.SendFileAsync(new FileStream(image.Path, FileMode.Open), path, embed: embed);
            }
        }
    }
}
