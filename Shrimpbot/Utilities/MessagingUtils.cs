using Discord;
using System;

namespace Shrimpbot.Utilities
{
    public class MessagingUtils
    {
        public const string InvalidParameterEmote = ":x:";
        public const string NoPermissionEmote = ":person_gesturing_no:";
        /// <summary>
        /// Returns an EmbedBuilder with some fields automatically populated for consistency
        /// </summary>
        public static EmbedBuilder GetShrimpbotEmbedBuilder()
        {
            var builder = new EmbedBuilder();
            builder.Color = new Color(51, 139, 193); // TODO: get from config file
            return builder;
        }
        public static bool CheckForPings(string text) => text.Contains("@everyone") || text.Contains("@here");
        public static string GetNoPermissionsString(string permission = null)
        {
            if (permission is null) return $"{NoPermissionEmote} You don't have permission to run this command.";
            else return $"{NoPermissionEmote} You do not have permission to {permission}.";
        }
        public static string GetServerNoPermissionsString(string permission = null)
        {
            if (permission is null) return $"{NoPermissionEmote} The server you're in doesn't allow you to run this command.";
            else return $"{NoPermissionEmote} The server you're in doesn't allow you to {permission}.";
        }

        public static string GetLengthString(TimeSpan time)
        {
            if (time.TotalDays > 365)
            {
                var years = time.TotalDays / 365;
                if (years > 1) return $"about {Math.Ceiling(years)} years";
                else return "about a year";
            }
            else if (time.TotalDays > 30)
            {
                var months = time.TotalDays / 30;
                if (months > 1) return $"about {Math.Ceiling(months)} months";
                else return "about a month";
            }
            else if (time.TotalDays >= 1)
            {
                if (time.TotalDays > 1) return $"about {Math.Ceiling(time.TotalDays)} days";
                else return "1 day";
            }
            else if (time.TotalHours >= 1)
            {
                if (time.TotalHours > 1) return $"{Math.Ceiling(time.TotalHours)} hours";
                else return "1 hour";
            }
            else if (time.TotalMinutes >= 1)
            {
                if (time.TotalMinutes > 1) return $"{Math.Ceiling(time.TotalMinutes)} minutes";
                else return "1 minute";
            }
            else if (time.TotalSeconds >= 1)
            {
                if (time.TotalSeconds > 1) return $"{Math.Ceiling(time.TotalSeconds)} seconds";
                else return "1 second";
            }
            else return "now";
        }
    }
}
