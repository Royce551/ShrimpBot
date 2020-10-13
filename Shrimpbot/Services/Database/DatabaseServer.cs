using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Shrimpbot.Services.Database
{
    public class DatabaseServer
    {
        public ulong Id { get; set; }
        public List<DatabaseServerUser> Users { get; set; }
        /// <summary>
        /// Whether commands that aren't guaranteed to return SFW results (like cute, with the 'online' parameter) are allowed to run.
        /// </summary>
        public bool AllowsPotentialNSFW { get; set; } = false;
        /// <summary>
        /// The channel where moderation events are logged.
        /// </summary>
        public ulong LoggingChannel { get; set; } = 0;
        /// <summary>
        /// The channel where ShrimpBot messages are sent.
        /// </summary>
        public ulong SystemChannel { get; set; } = 0;
    }
    public class DatabaseServerUser
    {
        public ulong Id { get; set; }
    }
}
