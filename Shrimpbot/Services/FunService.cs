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
                    5 => "<:bancat:765320197744623666>",
                    _ => "???"
                },
                Health = rng.Next(75, 105),
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
            builder.WithFooter("a - Attack; m - Use offensive magic; h - Use healing magic; f or quit - Flee");
            return builder;
        }
        public (ShrimpBattleTurnResults proResults, ShrimpBattleTurnResults eneResults) DoTurn(ShrimpBattleActionType action)
        {
            var rng = new Random();
            // enemy AI
            ShrimpBattleActionType turn;
            var defaultPerson = new ShrimpBattlePerson(); // A default person used to make some decisions
            if ((Enemy.Health < defaultPerson.Health / 2) && Enemy.Mana > 15) turn = ShrimpBattleActionType.Heal;
            else
            {
                if (Protagonist.Health > defaultPerson.Health / 2 && Enemy.Mana > 15) turn = ShrimpBattleActionType.UseMagic;
                else turn = ShrimpBattleActionType.Attack;
            }
                    
            // turn
            var protagonist = Protagonist;
            var enemy = Enemy;
            var proResults = action switch
            {
                ShrimpBattleActionType.Attack => Protagonist.Attack(rng, ref enemy),
                ShrimpBattleActionType.UseMagic => Protagonist.UseMagic(rng, ref enemy),
                ShrimpBattleActionType.Heal => Protagonist.Heal(rng, ref enemy),
                _ => throw new Exception("fucky wucky")
            };
            var eneResults = turn switch
            {
                ShrimpBattleActionType.Attack => Enemy.Attack(rng, ref protagonist),
                ShrimpBattleActionType.UseMagic => Enemy.UseMagic(rng, ref protagonist),
                ShrimpBattleActionType.Heal => Enemy.Heal(rng, ref protagonist),
                _ => throw new Exception("fucky wucky")
            };
            Protagonist = protagonist;
            Enemy = enemy;

            Protagonist.Mana += 3;
            Enemy.Mana += 3;

            return (proResults, eneResults);
        }
    }
    public class ShrimpBattlePerson
    {
        public int Health { get; set; } = 100;
        public int Mana { get; set; } = 0;
        public string Name { get; set; }
        public string Emote { get; set; }
        public bool IsDead() => Health <= 0;

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
            results.ManaUsed = 5;
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
        public ShrimpBattleTurnResults Heal(Random rng, ref ShrimpBattlePerson target)
        {
            var results = new ShrimpBattleTurnResults();
            results.ManaUsed = 15;
            if (Mana - results.ManaUsed > -1)
            {
                results.Response = $"{Name} used healing magic and gained 35 health.";
                Health += 35;
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
}
