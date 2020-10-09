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
    }
}
