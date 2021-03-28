using System;

namespace Shrimpbot.Services.Database
{
    public class DatabaseUser
    {
        /// <summary>
        /// Discord's ID for this user
        /// </summary>
        public ulong Id { get; set; }
        public BotPermissionLevel BotPermissions { get; set; } = BotPermissionLevel.User;
        public decimal Money { get; set; } = 1000;
        public int Cuteness { get; set; } = new Random().Next(1, 101);
        public DateTime DailyLastClaimed { get; set; } = DateTime.Now.AddDays(-1);
        public double DailyBonus { get; set; } = 1;
        /// <summary>
        /// The time zone offset from UTC for this user.
        /// </summary>
        public double TimeZoneOffset { get; set; } = 0;
    }
    public enum BotPermissionLevel
    {
        User,
        BotAdministrator,
        Owner
    }
}
