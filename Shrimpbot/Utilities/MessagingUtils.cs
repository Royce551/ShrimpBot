using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shrimpbot.Utilities
{
    public class MessagingUtils
    {
        /// <summary>
        /// Returns an EmbedBuilder with some fields automatically populated for consistency
        /// </summary>
        public static EmbedBuilder GetShrimpbotEmbedBuilder()
        {
            var builder = new EmbedBuilder();
            builder.Color = new Color(51, 139, 193); // TODO: get from config file
            return builder;
        }
        public static string GetNoPermissionsString(string permission = null)
        {
            if (permission is null) return ":x: You don't have permission to run this command.";
            else return $":x: You do not have permission to {permission}.";
        }
        public static string GetServerNoPermissionsString(string permission = null)
        {
            if (permission is null) return ":x: The server you're in doesn't allow you to run this command.";
            else return $":x: The server you're in doesn't allow you to {permission}.";
        }
    }
}
