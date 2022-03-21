using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shrimpbot.Services.Fun
{
    /// <summary>
    /// Logic behind Shrimpbot's fun commands
    /// </summary>
    public static class FunService
    {
        public static string GetEightBall()
        {
            var responses = new string[]
            {
                "It is certain.",
                "It is decidedly so.",
                "Without a doubt.",
                "Yes- definitely.",
                "You may rely on it.",
                "As I see it, yes.",
                "Most likely",
                "Outlook good.",
                "Yes.",
                "Signs point to yes.",
                "Reply hazy, try again.",
                "Ask again later.",
                "Better not tell you now.",
                "Cannot predict now.",
                "Concentrate and ask again.",
                "Don't count on it.",
                "My reply is no.",
                "My sources say no.",
                "Outlook not so good.",
                "Very doubtful."
            };

            return responses[Random.Shared.Next(0, responses.Length - 1)];
        }

        public static string Uwuify(string text)
        {
            var endings = new string[]
            {
                " :P",
                " :3",
                " x3",
                " XDDDD",
                "（＾ｖ＾）",
                "(●´ω｀●)",
                " uwu",
                " owo",
                " nyaa~",
                " wan wan~!",
                "~",
                " nyaa~",
                "... fwendo",
            };
            var uwuifiedText = string.Empty;
            string InsertWeebStuff()
            {
                return $"{endings[Random.Shared.Next(0, endings.Length - 1)]}";
            }
            foreach (char x in text)
            {
                uwuifiedText += x switch
                {
                    'r' => 'w',
                    'l' => 'w',
                    'R' => 'W',
                    'L' => 'W',
                    _ => x
                };
            }

            uwuifiedText = uwuifiedText.Replace("no", "nu");
            uwuifiedText = uwuifiedText.Replace("have", "haz");
            uwuifiedText = uwuifiedText.Replace("has", "haz");
            uwuifiedText = uwuifiedText.Replace("you", "uu");

            uwuifiedText = Regex.Replace(uwuifiedText, @"\b[Tt]he\b", "da");

            uwuifiedText += InsertWeebStuff() + ' ';
            return uwuifiedText;
        }

        public static (string spaceParagraph, string normalParagraph) GetRandomParagraphWithSpaces()
        {
            if (!Directory.Exists("Paragraphs")) Directory.CreateDirectory("Paragraphs");
            var textFiles = Directory.EnumerateFiles("Paragraphs", "*.txt").ToList();
            var paragraphs = new List<string>();
            foreach (var textFile in textFiles)
            {
                paragraphs.Add(File.ReadAllText(textFile));
            }
            var selectedParagraph = paragraphs[Random.Shared.Next(0, paragraphs.Count - 1)];
            var antiCheatParagraph = selectedParagraph;
            for (int i = 0; i < 20; i++)
            {                                                              // that's a 0 width space (U+200B) v
                antiCheatParagraph = antiCheatParagraph.Insert(Random.Shared.Next(antiCheatParagraph.Length), "​");
            }
            return (antiCheatParagraph, selectedParagraph);
        }
        public static ShrimpBattle CreateBattle(string userName)
        {
            var battle = new ShrimpBattle();
            battle.Protagonist = new ShrimpBattlePerson
            {
                Name = userName,
                Emote = ":smiley:"
            };
            int enemyType = Random.Shared.Next(1, 6);
            battle.Enemy = new ShrimpBattlePerson
            {
                Name = enemyType switch
                {
                    1 => "Jeremy",
                    2 => "theBeat",
                    3 => "Zombie",
                    4 => "Vampire",
                    5 => "Random Weeaboo",
                    _ => "fucky wucky"
                },
                Emote = enemyType switch
                {
                    1 => ":man:",
                    2 => ":musical_note:",
                    3 => ":zombie:",
                    4 => ":vampire:",
                    5 => "<:bancat:765320197744623666>",
                    _ => "???"
                },
                Health = Random.Shared.Next(75, 105),
            };
            return battle;
        }
        public static MultiplayerShrimpBattle CreateBattleMultiplayer(string player1Name, string player2Name)
        {
            return new MultiplayerShrimpBattle
            {
                Protagonist = new ShrimpBattlePerson
                {
                    Name = player1Name,
                    Emote = ":smiley:"
                },
                Enemy = new ShrimpBattlePerson
                {
                    Name = player2Name,
                    Emote = ":upside_down:"
                }
            };
        }
    }
   
}
