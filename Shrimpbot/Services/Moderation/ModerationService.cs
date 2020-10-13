using Discord;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shrimpbot.Services.Moderation
{
    public class ModerationService
    {
        public static BotModerationNavigator CreateBotModerationNavigator(DatabaseServer server)
        {
            return new BotModerationNavigator { Server = server };
        }
    }
    public class BotModerationNavigator
    {
        public BotModerationPage CurrentPage { get; set; }
        public DatabaseServer Server { get; set; }
        public void Navigate(Property property)
        {
            CurrentPage = property switch
            {
                Property.AllowsPotentialNSFW => new BotModerationPage
                {
                    PropertyName = "Allow potential NSFW in non- NSFW channels",
                    PropertyDescription = 
                    $"Whether commands that aren't guaranteed to return SFW results (like cute, with the 'online' parameter) are allowed to run.\n" +
                    $"Currently {Server.AllowsPotentialNSFW}.",
                    Prompt =
                    "**set true** - To allow potential NSFW\n" +
                    "**set false** - To block potential NSFW",
                    Set = (string arg) =>
                    {
                        if (arg.ToLower() == "true")
                        {
                            Server.AllowsPotentialNSFW = true;
                            return true;
                        }
                        if (arg.ToLower() == "false")
                        {
                            Server.AllowsPotentialNSFW = false;
                            return true;
                        }
                        else return false;
                    }
                },
                Property.LoggingChannel => new BotModerationPage
                {
                    PropertyName = "Logging Channel",
                    PropertyDescription = 
                    $"The channel where moderation events are logged.\n" +
                    $"Currently set to {Server.LoggingChannel}",
                    Prompt = "set [The ID of the channel to set]",
                    Set = (string arg) =>
                    {
                        if (uint.TryParse(arg, out uint result))
                        {
                            Server.LoggingChannel = result;
                            return true;
                        }
                        else return false;
                    }
                },
                Property.SystemChannel => new BotModerationPage
                {
                    PropertyName = "System Channel",
                    PropertyDescription = 
                    $"The channel where ShrimpBot messages are sent.\n" +
                    $"Currently set to {Server.SystemChannel}",
                    Prompt = "set [The ID of the channel to set]",
                    Set = (string arg) =>
                    {
                        if (uint.TryParse(arg, out uint result))
                        {
                            Server.SystemChannel = result;
                            return true;
                        }
                        else return false;
                    }
                },
                _ => throw new Exception("fucky wucky")
            };
        }
        public bool Set(string response) => CurrentPage.Set.Invoke(response);
        public EmbedBuilder GetFormattedCurrentPage(IUser user)
        {
            var builder = MessagingUtils.GetShrimpbotEmbedBuilder();
            builder.WithAuthor(user);
            builder.AddField(CurrentPage.PropertyName, 
                $"{CurrentPage.PropertyDescription}\n\n" +
                $"Type...\n" +
                $"{CurrentPage.Prompt}\n" +
                $"to set this property.");
            builder.WithFooter($"'quit' - Exit; 'back' - Back");
            return builder;
        }
    }
    public class BotModerationPage
    {
        public string PropertyName { get; set; }
        public string PropertyDescription { get; set; }
        public string Prompt { get; set; }
        public Func<string, bool> Set { get; set; }
    }
    public enum Property
    {
        AllowsPotentialNSFW,
        LoggingChannel,
        SystemChannel
    }
    public enum PropertyType
    {
        String,
        Bool
    }
}
