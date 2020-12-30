using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;

namespace Shrimpbot.Services
{
    /// <summary>
    /// Provides multi page "navigators" ideal for doing configuration.
    /// </summary>
    public class ManagementService
    {
        public static UserSettingsNavigator CreateUserSettingsNavigator(DatabaseUser user) => new UserSettingsNavigator { User = user };
        public static BotModerationNavigator CreateBotModerationNavigator(DatabaseServer server, SocketGuild discordServer) => new BotModerationNavigator { Server = server, DiscordServer = discordServer };
    }
    public abstract class Navigator
    {
        public ManagementPage CurrentPage { get; set; }
        public bool Set(string response) => CurrentPage.Set.Invoke(response);
        public List<ManagementPage> Pages { get; set; }
        public abstract EmbedBuilder GetHomePage(SocketCommandContext context);
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
    public class UserSettingsNavigator : Navigator
    {
        public DatabaseUser User { get; init; }
        public override EmbedBuilder GetHomePage(SocketCommandContext context)
        {
            var initialPromptEmbedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            initialPromptEmbedBuilder.WithAuthor(context.User);
            initialPromptEmbedBuilder.WithDescription($"User Settings");
            initialPromptEmbedBuilder.AddField("The following properties are available to edit:",
                $"**1** - Nothing here yet! Work in progress!");
            initialPromptEmbedBuilder.WithFooter("'quit' - Exit");
            return initialPromptEmbedBuilder;
        }
        public void Navigate(UserProperty property)
        {
            CurrentPage = property switch
            {
                UserProperty.TimeZone => new ManagementPage
                {
                    PropertyName = "Time zone",
                    PropertyDescription = "Do not attempt to set this or everything will explode",
                    Prompt =
                    "set [Time zone ID, for example 'Central Standard Time', or 'US Mountain Standard Time']"
                },
                _ => throw new Exception("fucky wucky")
            };
        }
    }
    public class BotModerationNavigator : Navigator
    {
        public DatabaseServer Server { get; init; }
        public SocketGuild DiscordServer {get; init;}
        public override EmbedBuilder GetHomePage(SocketCommandContext context)
        {
            var initialPromptEmbedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            initialPromptEmbedBuilder.WithAuthor(context.User);
            initialPromptEmbedBuilder.WithDescription($"{context.Guild.Name} Settings");
            initialPromptEmbedBuilder.AddField("The following properties are available to edit:",
                $"**1** - Allow potential NSFW in non- NSFW channels - {Server.AllowsPotentialNSFW}\n" +
                $"**2** - Logging Channel - {Server.LoggingChannel}\n" +
                $"**3** - System Channel - {Server.SystemChannel}\n");
            initialPromptEmbedBuilder.WithFooter("'quit' - Exit");
            return initialPromptEmbedBuilder;
        }
        public void Navigate(ServerProperty property)
        {
            CurrentPage = property switch
            {
                ServerProperty.AllowsPotentialNSFW => new ManagementPage
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
                ServerProperty.LoggingChannel => new ManagementPage
                {
                    PropertyName = "Logging Channel",
                    PropertyDescription =
                    $"The channel where moderation events are logged.\n" +
                    $"Currently set to {Server.LoggingChannel}",
                    Prompt = "set [The name or ID of the channel to set]",
                    Set = (string arg) =>
                    {
                        foreach (var server in DiscordServer.Channels)
                        {
                            if (server.Name == arg)
                            {
                                Server.LoggingChannel = server.Id;
                                return true;
                            }
                        }
                        if (ulong.TryParse(arg, out ulong result))
                        {
                            Server.LoggingChannel = result;
                            return true;
                        }
                        else return false;
                    }
                },
                ServerProperty.SystemChannel => new ManagementPage
                {
                    PropertyName = "System Channel",
                    PropertyDescription =
                    $"The channel where ShrimpBot messages are sent.\n" +
                    $"Currently set to {Server.SystemChannel}",
                    Prompt = "set [The ID of the channel to set]",
                    Set = (string arg) =>
                    {
                        foreach (var server in DiscordServer.Channels)
                        {
                            if (server.Name == arg)
                            {
                                Server.SystemChannel = server.Id;
                                return true;
                            }
                        }
                        if (ulong.TryParse(arg, out ulong result))
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
    }
    public class ManagementPage
    {
        public string PropertyName { get; set; }
        public string PropertyDescription { get; set; }
        public string Prompt { get; set; }
        public Func<string, bool> Set { get; set; }
    }
    public enum ServerProperty
    {
        AllowsPotentialNSFW,
        LoggingChannel,
        SystemChannel
    }
    public enum UserProperty
    {
        TimeZone
    }
    public enum PropertyType
    {
        String,
        Bool
    }
}
