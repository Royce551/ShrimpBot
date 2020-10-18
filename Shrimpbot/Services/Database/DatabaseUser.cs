using System;
using System.Collections.Generic;
using System.Text;

namespace Shrimpbot.Services.Database
{
    public class DatabaseUser
    {
        public ulong Id { get; set; }
        public BotPermissionLevel BotPermissions { get; set; } = BotPermissionLevel.User;
        public decimal Money { get; set; } = 1000;
        public int Cuteness { get; set; } = -1;
        public DateTime DailyLastClaimed { get; set; } = DateTime.Now.AddDays(-1);
        public double DailyBonus { get; set; } = 1;
    }
    public enum BotPermissionLevel
    {
        User,
        BotAdministrator,
        Owner
    }
}
