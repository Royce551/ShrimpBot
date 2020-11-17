using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services;
using Shrimpbot.Services.Configuration;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Threading.Tasks;

namespace Shrimpbot.Modules
{
    [Name("Fun")]
    [Summary("fun n' games n' stuff")]
    public class FunModule : InteractiveBase
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService CommandService { get; set; }
        public ConfigurationFile Config { get; set; }

        [Command("8ball", ignoreExtraArgs: true)]
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
                user.Cuteness = new Random().Next(1, 101);
                DatabaseManager.WriteUser(user);
            }
            if (runner is null) await ReplyAsync($":two_hearts: Your cuteness level is {user.Cuteness}/100.");
            else await ReplyAsync($":two_hearts: {runner.Username}'s cuteness is {user.Cuteness}/100.");
        }
        [Command("uwuify")]
        [Summary("uwu")]
        public async Task Uwuify(string text)
        {
            if (!MessagingUtils.CheckForPings(text)) await ReplyAsync(FunService.Uwuify(text));
            else await ReplyAsync($"{MessagingUtils.NoPermissionEmote} You can't ping everyone.");
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
                    var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 0, 0, -1)); // Infinite timeout
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
                runner.Money += (decimal)Math.Round(maxguesses * 10 * payMultiplier);
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

                (ShrimpBattleTurnResults proResults, ShrimpBattleTurnResults eneResults) turnResults;
                if (response.Content == "a") turnResults = battle.DoTurn(ShrimpBattleActionType.Attack);
                else if (response.Content == "m") turnResults = battle.DoTurn(ShrimpBattleActionType.UseMagic);
                else if (response.Content == "h") turnResults = battle.DoTurn(ShrimpBattleActionType.Heal);
                else if (response.Content == "f" || response.Content == "quit")
                {
                    await ReplyAsync(":person_running: You fled.");
                    return;
                }
                else
                {
                    await ReplyAsync(":x: That's not a valid action. If you need to quit, type 'quit'.");
                    continue;
                }
                await ReplyAsync(
                    $"{turnResults.proResults.Response}\n**{turnResults.eneResults.Response}**\n\n" +
                    $"**You** deal **{turnResults.proResults.DamageDealt} damage** and use {turnResults.proResults.ManaUsed} mana.\n" +
                    $"**{battle.Enemy.Name}** deals **{turnResults.eneResults.DamageDealt} damage** and uses {turnResults.eneResults.ManaUsed} of their mana.");
            }
            await ReplyAsync($":tada: You win! You got 50 {Config.Currency} for your performance.");
            var user = DatabaseManager.GetUser(Context.User.Id);
            user.Money += 50;
            DatabaseManager.WriteUser(user);
        }
        [Command("battlemultiplayer", RunMode = RunMode.Async)]
        [Summary("Battle your friends")]
        public async Task BattleMultiplayer()
        {
            SocketUser player1 = Context.User;
            SocketUser player2 = null;
            bool foundPlayer2 = false;
            await ReplyAsync("Player 2, say 'join' to join! (Say 'quit' to stop searching.)");
            while (!foundPlayer2)
            {
                var response = await NextMessageAsync(fromSourceUser: false, timeout: new TimeSpan(0, 0, 0, 0, -1));
                if (response.Content == "join" && response.Author != player1)
                {
                    player2 = response.Author;
                    foundPlayer2 = true;
                }
                else if (response.Content == "quit")
                {
                    await ReplyAsync("Quit.");
                    return;
                }
            }
            var battle = FunService.CreateBattleMultiplayer(player1.Username, player2.Username);
            while (true)
            {
                if (battle.Protagonist.IsDead())
                {
                    await ReplyAsync($"{battle.Enemy.Name} wins!");
                    return;
                }
                // TODO: reuse code and make this not be a pain to maintain in the future
                // Player 1's turn
                while (battle.Turn == ShrimpBattleTurn.Player1)
                {
                    SocketMessage player1Response;
                    await ReplyAsync($"{battle.Protagonist.Name}, it's your turn!", embed: battle.GetFormattedStatus(player1).Build());
                    while (true)
                    {
                        player1Response = await NextMessageAsync(fromSourceUser: false, timeout: new TimeSpan(0, 0, 0, 0, -1));
                        if (player1Response.Author == player1) break;
                    }
                    ShrimpBattleTurnResults player1TurnResults;
                    if (player1Response.Content == "a") player1TurnResults = battle.DoTurn(ShrimpBattleActionType.Attack);
                    else if (player1Response.Content == "m") player1TurnResults = battle.DoTurn(ShrimpBattleActionType.UseMagic);
                    else if (player1Response.Content == "h") player1TurnResults = battle.DoTurn(ShrimpBattleActionType.Heal);
                    else if (player1Response.Content == "f" || player1Response.Content == "quit")
                    {
                        await ReplyAsync(":person_running: You fled.");
                        return;
                    }
                    else
                    {
                        await ReplyAsync(":x: That's not a valid action. If you need to quit, type 'quit'.");
                        continue;
                    }
                    await ReplyAsync(
                        $"{player1TurnResults.Response}\n" +
                        $"**{battle.Protagonist.Name}** dealt **{player1TurnResults.DamageDealt} damage** and used {player1TurnResults.ManaUsed} mana.");
                    player1Response = null;
                }
                if (battle.Enemy.IsDead())
                {
                    await ReplyAsync($"{battle.Protagonist.Name} wins!");
                    return;
                }
                // Player 2's turn
                while (battle.Turn == ShrimpBattleTurn.Player2)
                {
                    SocketMessage player2Response;
                    await ReplyAsync($"{battle.Enemy.Name}, it's your turn!", embed: battle.GetFormattedStatus(player1).Build());
                    while (true)
                    {
                        player2Response = await NextMessageAsync(fromSourceUser: false, timeout: new TimeSpan(0, 0, 0, 0, -1));
                        if (player2Response.Author == player2) break;
                    }
                    ShrimpBattleTurnResults player2TurnResults;
                    if (player2Response.Content == "a") player2TurnResults = battle.DoTurn(ShrimpBattleActionType.Attack);
                    else if (player2Response.Content == "m") player2TurnResults = battle.DoTurn(ShrimpBattleActionType.UseMagic);
                    else if (player2Response.Content == "h") player2TurnResults = battle.DoTurn(ShrimpBattleActionType.Heal);
                    else if (player2Response.Content == "f" || player2Response.Content == "quit")
                    {
                        await ReplyAsync(":person_running: You fled.");
                        return;
                    }
                    else
                    {
                        await ReplyAsync(":x: That's not a valid action. If you need to quit, type 'quit'.");
                        continue;
                    }
                    await ReplyAsync(
                        $"{player2TurnResults.Response}\n" +
                        $"**{battle.Enemy.Name}** dealt **{player2TurnResults.DamageDealt} damage** and used {player2TurnResults.ManaUsed} mana.");
                    player2Response = null;
                    break;
                }
            }
        }
        [Command("typerace", RunMode = RunMode.Async)]
        [Summary("mechanical keyboard sounds intensify")]
        public async Task TypeRace()
        {
            var paragraph = FunService.GetRandomParagraph();
            var wordCount = paragraph.Length / 5; // words in typing are 5 letters, regardless of the actual words
            var startTime = DateTime.Now;
            await ReplyAsync($"You have idk minutes to type this thing! glhf\n\n{paragraph}");
            while (true)
            {
                var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 0, 0, -1));
                if (response.Content == paragraph)
                {
                    var timeTaken = DateTime.Now - startTime;
                    var wpm = wordCount / timeTaken.TotalMinutes;
                    if (timeTaken.TotalMilliseconds <= wordCount * 200)
                    {
                        await ReplyAsync($"I CAN SEE THROUGH YOUR BS YOU HACKER");
                        continue;
                    }
                    await ReplyAsync($"Congrats! You took {timeTaken:mm\\:ss} and typed {Math.Round(wpm)} wpm.");
                    return;
                }
                else await ReplyAsync("Looks like you made a mistake.");
            }
        }
        [Command("cute")]
        [Summary("Gets a random cute image")]
        public async Task Cute(string type = "all", string source = "curated")
        {
            var imageType = CuteService.ParseImageType(type);
            var imageSource = CuteService.ParseImageSource(source);
            if (Context.Guild != null)
            {
                var server = DatabaseManager.GetServer(Context.Guild.Id);
                if (imageSource == ImageSource.Online && !server.AllowsPotentialNSFW)
                {
                    await ReplyAsync(MessagingUtils.GetServerNoPermissionsString());
                    return;
                }
            }

            CuteService.GetImage(imageSource, imageType).SendEmbed(Context);
        }

    }
}
