using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Shrimpbot.Services.Fun
{
    /// <summary>
    /// Logic behind Shrimpbot's fun commands
    /// </summary>
    public static class FunService
    {
        static readonly Random rng = new Random();

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

            return responses[rng.Next(0, responses.Length - 1)];
        }

        public static string Uwuify(string text)
        {
            var kaomoji = new string[]
            {
                "(◡ ω ◡)",
                "(˘ω˘)",
                "(⑅˘꒳˘)",
                "(˘ᵕ˘)",
                "(˘˘˘)",
                "( ᴜ ω ᴜ )",
                "( ´ω` )۶",
                "(„ᵕᴗᵕ„)",
                "(ㅅꈍ ˘ ꈍ)",
                "(⑅˘꒳˘)",
                "( ｡ᵘ ᵕ ᵘ ｡)",
                "( ᵘ ꒳ ᵘ ✼)",
                "( ˘ᴗ˘ )",
                "(ᵕᴗ ᵕ⁎)",
                "*:･ﾟ✧(ꈍᴗꈍ)✧･ﾟ:*",
                "*˚*(ꈍ ω ꈍ).₊̣̇.",
                "(。U ω U。)",
                "(U ᵕ U❁)",
                "(◦ᵕ ˘ ᵕ◦)",
                "( ˊ.ᴗˋ )",
                "(灬´ᴗ`灬)",
                "uwu",
                "owo",
            };
            var weebPhrases = new string[]
            {
                " nyaa~",
                " wan wan~!",
                "~",
                " Nyaa~"
            };
            var uwuifiedText = string.Empty;
            string InsertWeebStuff()
            {
                var number = rng.Next(0, 2);
                if (number == 0) return $" {kaomoji[rng.Next(0, kaomoji.Length - 1)]}";
                else if (number == 1) return weebPhrases[rng.Next(0, weebPhrases.Length - 1)];
                else return string.Empty;
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
            uwuifiedText = uwuifiedText.Replace("! ", '!' + InsertWeebStuff() + ' ');
            uwuifiedText = uwuifiedText.Replace(". ", '.' + InsertWeebStuff() + ' ');
            uwuifiedText = uwuifiedText.Replace("? ", '?' + InsertWeebStuff() + ' ');
            return uwuifiedText;
        }

        public static string GetRandomParagraph()
        {
            if (!Directory.Exists("Paragraphs")) Directory.CreateDirectory("Paragraphs");
            var textFiles = Directory.EnumerateFiles("Paragraphs", "*.txt").ToList();
            var paragraphs = new List<string>();
            foreach (var textFile in textFiles)
            {
                paragraphs.Add(File.ReadAllText(textFile));
            }
            return paragraphs[rng.Next(0, paragraphs.Count - 1)];
        }
        public static ShrimpBattle CreateBattle(string userName)
        {
            var battle = new ShrimpBattle();
            battle.Protagonist = new ShrimpBattlePerson
            {
                Name = userName,
                Emote = ":smiley:"
            };
            int enemyType = rng.Next(1, 6);
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
                Health = rng.Next(75, 105),
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
