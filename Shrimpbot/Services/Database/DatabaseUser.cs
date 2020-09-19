using System;
using System.Collections.Generic;
using System.Text;

namespace Shrimpbot.Services.Database
{
    public class DatabaseUser
    {
        public ulong Id { get; set; }
        public int Money { get; set; } = 1000;
    }
}
