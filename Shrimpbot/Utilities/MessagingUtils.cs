using Discord;

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
    }
}
