using Discord;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Shrimpbot.Services
{
    public class FunService
    {
        
        public static string GetEightBall()
        {
            Random random = new Random();
            int number = random.Next(1, 20);
            return number switch
            {
                1 => "It is certain.",
                2 => "It is decidedly so.",
                3 => "Without a doubt.",
                4 => "Yes- definitely.",
                5 => "You may rely on it.",
                6 => "As I see it, yes.",
                7 => "Most likely",
                8 => "Outlook good.",
                9 => "Yes.",
                10 => "Signs point to yes.",
                11 => "Reply hazy, try again.",
                12 => "Ask again later.",
                13 => "Better not tell you now.",
                14 => "Cannot predict now.",
                15 => "Concentrate and ask again.",
                16 => "Don't count on it.",
                17 => "My reply is no.",
                18 => "My sources say no.",
                19 => "Outlook not so good.",
                20 => "Very doubtful.",
                _ => "the dev did a fucky wucky"
            };

        }

        public static ShrimpBattle CreateBattle(string userName)
        {
            var battle = new ShrimpBattle();
            battle.Protagonist = new ShrimpBattlePerson
            {
                Name = userName,
                Emote = ":smiley:"
            };
            var rng = new Random();
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
                    5 => ":bancat:",
                    _ => "???"
                },
                Health = rng.Next(55, 105),
                Mana = rng.Next(40, 60)
            };    
            return battle;
        }
    }
    public class ShrimpBattle
    {
        public ShrimpBattlePerson Protagonist { get; set; }
        public ShrimpBattlePerson Enemy { get; set; }
        public EmbedBuilder GetFormattedStatus(IUser user)
        {
            var builder = MessagingUtils.GetShrimpbotEmbedBuilder();
            builder.WithAuthor(user);
            builder.WithDescription(@$"{Protagonist.Emote}\_\_\_\_\_\_\_\_\_\_\_\__{Enemy.Emote}");
            builder.AddField(Protagonist.Name,
                $":blue_heart: **Health**: {Protagonist.Health}\n" +
                $":magic_wand: **Mana**: {Protagonist.Mana}\n");
            builder.AddField(Enemy.Name,
                $":hearts: **Health**: {Enemy.Health}\n" +
                $":magic_wand: **Mana**: {Enemy.Mana}\n");
            builder.WithFooter("a - Attack; m - Use offensive magic; h - Use healing magic; r or quit - Run");
            return builder;
        }
        public (int proDamageDealt, int proManaUsed, int enemyDamageDealt, int enemyManaUsed, string response) DoTurn(ShrimpBattleActionType action)
        {
            var rng = new Random();
            Array values = Enum.GetValues(typeof(ShrimpBattleActionType));
            ShrimpBattleActionType turn = (ShrimpBattleActionType)values.GetValue(rng.Next(values.Length));
            int proDamageDealt = 0;
            int proManaUsed = 0;
            string response;
            // Protagonist attacks enemy
            if (action == ShrimpBattleActionType.Attack)
            {
                proDamageDealt = rng.Next(1, 11);
                Enemy.Health -= proDamageDealt;
                response = $"You hit {Enemy.Name} with your mighty stick.";
            }
            else if (action == ShrimpBattleActionType.UseMagic)
            {
                proDamageDealt = rng.Next(10, 21);
                proManaUsed = 5;
                if (Protagonist.Mana - proManaUsed > -1)
                {
                    Enemy.Health -= proDamageDealt;
                    Protagonist.Mana -= proManaUsed;
                    response = $"You cast magic on {Enemy.Name}.";
                }
                else
                {
                    response = $"You didn't have enough mana to use your magic, so it didn't do anything.";
                    proDamageDealt = 0;
                    proManaUsed = 0;
                }
            }
            else
            {
                proManaUsed = 15;
                if (Protagonist.Mana - proManaUsed > -1)
                {
                    response = $"You cast healing magic on yourself and gained 35 health.";
                    Protagonist.Health += 35;
                    Protagonist.Mana -= proManaUsed;
                }
                else
                {
                    response = $"You didn't have enough mana to use your magic, so it didn't do anything.";
                    proDamageDealt = 0;
                    proManaUsed = 0;
                }
            }
            // Enemy attacks protagonist
            int enemyDamageDealt = 0;
            int enemyManaUsed = 0;
            if (turn == ShrimpBattleActionType.Attack) // TODO: reuse code
            {
                enemyDamageDealt = rng.Next(1, 11);
                Protagonist.Health -= enemyDamageDealt;
            }
            else
            {
                enemyDamageDealt = rng.Next(10, 21);
                enemyManaUsed = 5;
                if (Enemy.Mana - enemyManaUsed > -1)
                {
                    Protagonist.Health -= enemyDamageDealt;
                    Enemy.Mana -= enemyManaUsed;
                }
                else
                {
                    enemyDamageDealt = 0;
                    enemyManaUsed = 0;
                }
            }
            return (proDamageDealt, proManaUsed, enemyDamageDealt, enemyManaUsed, response);
        }
    }
    public class ShrimpBattlePerson
    {
        public int Health { get; set; } = 100;
        public int Mana { get; set; } = 25;
        public string Name { get; set; }
        public string Emote { get; set; }
        public bool IsDead() => Health <= 0;
    }
    public enum ShrimpBattleActionType
    {
        Attack,
        UseMagic,
        Heal
    }
}
