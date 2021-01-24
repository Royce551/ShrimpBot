using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrimpbot.Utilities
{
    public class PermissionsUtils
    {
        /// <summary>
        /// <see cref="RequireUserPermissionAttribute"/>, but it isn't an attribute
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="permission">The permission the user needs</param>
        /// <returns>True if the user has permissions, else false. Also returns false if the command wasn't executed in a guild.</returns>
        public static bool CheckForPermissions(ICommandContext context, GuildPermission permission)
        {
            if (context.User is not IGuildUser guildUser) return false;
            if (guildUser.GuildPermissions.Has(permission)) return true;
            else return false;
        }
        /// <summary>
        /// <see cref="RequireUserPermissionAttribute"/>, but it isn't an attribute
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="permission">The permission the user needs</param>
        /// <returns>True if the user has permissions, else false. Also returns false if the command wasn't executed in a guild.</returns>
        public static bool CheckForPermissions(ICommandContext context, ChannelPermission permission)
        {
            var guildUser = context.User as IGuildUser;
            ChannelPermissions perms;
            if (context.Channel is IGuildChannel guildChannel)
                perms = guildUser.GetPermissions(guildChannel);
            else
                perms = ChannelPermissions.All(context.Channel);

            if (perms.Has(permission)) return true;
            else return false;
        }
    }
}
