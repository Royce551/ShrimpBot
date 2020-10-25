using Discord;
using Shrimpbot.Utilities;
using System;

namespace Shrimpbot.Services
{
    public static class FunService
    {
        static Random rng = new Random();

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
            foreach (char x in text)
            {
                uwuifiedText += x switch
                {
                    'r' => 'l',
                    'l' => 'w',
                    'R' => 'W',
                    'L' => 'W',
                    _ => x
                };
            }
            var number = rng.Next(0, 2);
            if (number == 0) uwuifiedText += $" {kaomoji[rng.Next(0, kaomoji.Length - 1)]}";
            else if (number == 1) uwuifiedText += weebPhrases[rng.Next(0, weebPhrases.Length - 1)];
            return uwuifiedText;
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
    public class ShrimpBattle
    {
        public ShrimpBattlePerson Protagonist { get; set; }
        public ShrimpBattlePerson Enemy { get; set; }
        public int Turns { get; set; } = 1;
        public bool InBlitzMode { get; private set; } = false;
        public EmbedBuilder GetFormattedStatus(IUser user)
        {
            var builder = MessagingUtils.GetShrimpbotEmbedBuilder();
            builder.WithAuthor(user);
            if (!InBlitzMode) builder.WithDescription($"{Protagonist.Emote}\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\_\\__{Enemy.Emote}\n**Turn {Turns}**");
            else builder.WithDescription($"{Protagonist.Emote}🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥{Enemy.Emote}\n**Turn {Turns}**\n**In blitz mode! Healing magic is banned!**");
            builder.AddField(Protagonist.Name,
                $":blue_heart: **Health**: {Protagonist.Health}\n" +
                $":magic_wand: **Mana**: {Protagonist.Mana}\n");
            builder.AddField(Enemy.Name,
                $":hearts: **Health**: {Enemy.Health}\n" +
                $":magic_wand: **Mana**: {Enemy.Mana}\n");
            builder.WithFooter("a - Attack; m - Use offensive magic; h - Use healing magic; f or quit - Flee");
            return builder;
        }
        public (ShrimpBattleTurnResults proResults, ShrimpBattleTurnResults eneResults) DoTurn(ShrimpBattleActionType action)
        {
            if (Turns >= 15) InBlitzMode = true;
            var rng = new Random();
            // enemy AI
            ShrimpBattleActionType turn;
            var defaultPerson = new ShrimpBattlePerson(); // A default person used to make some decisions
            if ((Enemy.Health < defaultPerson.Health / 2) && Enemy.Mana > ShrimpBattlePerson.ManaNeededForHealingMagic && !InBlitzMode) turn = ShrimpBattleActionType.Heal;
            else
            {
                if (Protagonist.Health < defaultPerson.Health / 2 && Enemy.Mana > ShrimpBattlePerson.ManaNeededForOffensiveMagic) turn = ShrimpBattleActionType.UseMagic;
                else turn = ShrimpBattleActionType.Attack;
            }
                    
            // turn
            var protagonist = Protagonist;
            var enemy = Enemy;
            var proResults = PerformActionForType(action, rng, protagonist, enemy, this);
            var eneResults = PerformActionForType(turn, rng, enemy, protagonist, this);
            Protagonist = protagonist;
            Enemy = enemy;

            Protagonist.Mana += 3;
            Enemy.Mana += 3;

            Turns++;
            return (proResults, eneResults);
        }
        public static ShrimpBattleTurnResults PerformActionForType(ShrimpBattleActionType type, Random rng, ShrimpBattlePerson attacker, ShrimpBattlePerson target, ShrimpBattle battle) => type switch
        {
            ShrimpBattleActionType.Attack => attacker.Attack(rng, ref target),
            ShrimpBattleActionType.UseMagic => attacker.UseMagic(rng, ref target),
            ShrimpBattleActionType.Heal => attacker.Heal(battle),
            _ => throw new Exception("fucky wucky")
        };
    }
    public class MultiplayerShrimpBattle : ShrimpBattle
    {
        public ShrimpBattleTurn Turn { get; private set; } = ShrimpBattleTurn.Player1;
        public new ShrimpBattleTurnResults DoTurn(ShrimpBattleActionType action)
        {
            var rng = new Random();
            ShrimpBattleTurnResults results;
            if (Turn == ShrimpBattleTurn.Player1)
            {
                results = PerformActionForType(action, rng, Protagonist, Enemy, this);
                Turn = ShrimpBattleTurn.Player2;
            }
            else
            {
                results = PerformActionForType(action, rng, Enemy, Protagonist, this);
                Turn = ShrimpBattleTurn.Player1;

                // Complete turn completed
                Protagonist.Mana += 3;
                Enemy.Mana += 3;
                Turns++;
            }
            return results;
        }
    }
    public class ShrimpBattlePerson
    {
        public int Health { get; set; } = 100;
        public int Mana { get; set; } = 0;
        public string Name { get; set; }
        public string Emote { get; set; }
        public bool IsDead() => Health <= 0;

        public const int ManaNeededForOffensiveMagic = 5;
        public const int ManaNeededForHealingMagic = 30;
        public ShrimpBattleTurnResults Attack(Random rng, ref ShrimpBattlePerson target)
        {
            var results = new ShrimpBattleTurnResults();
            results.DamageDealt = rng.Next(1, 11);
            target.Health -= results.DamageDealt;
            results.Response = $"{Name} hit {target.Name} with their mighty sword.";
            return results;
        }
        public ShrimpBattleTurnResults UseMagic(Random rng, ref ShrimpBattlePerson target)
        {
            var results = new ShrimpBattleTurnResults();
            results.DamageDealt = rng.Next(10, 21);
            results.ManaUsed = ManaNeededForOffensiveMagic;
            if (Mana - results.ManaUsed > -1)
            {
                target.Health -= results.DamageDealt;
                Mana -= results.ManaUsed;
                results.Response = $"{Name} cast magic on {target.Name}.";
            }
            else
            {
                results.Response = $"{Name} didn't have enough mana to use their magic, so it didn't do anything.";
                results.DamageDealt = 0;
                results.ManaUsed = 0;
            }
            return results;
        }
        public ShrimpBattleTurnResults Heal(ShrimpBattle battle)
        {
            var results = new ShrimpBattleTurnResults();
            if (battle.InBlitzMode)
            {
                results.Response = "The battle is in blitz mode! No healing magic!";
                return results;
            }
            results.ManaUsed = ManaNeededForHealingMagic;
            if (Mana - results.ManaUsed > -1)
            {
                results.Response = $"{Name} used healing magic and gained 30 health.";
                Health += 30;
                Mana -= results.ManaUsed;
            }
            else
            {
                results.Response = $"{Name} didn't have enough mana to use their magic, so it didn't do anything.";
                results.DamageDealt = 0;
                results.ManaUsed = 0;
            }
            return results;
        }
    }
    public class ShrimpBattleTurnResults
    {
        public int DamageDealt { get; set; }
        public int ManaUsed { get; set; }
        public string Response { get; set; }
    }
    public enum ShrimpBattleActionType
    {
        Attack,
        UseMagic,
        Heal
    }
    public enum ShrimpBattleTurn
    {
        Player1,
        Player2
    }
}
