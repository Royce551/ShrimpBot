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
                $"**1** - Time Zone: {User.TimeZoneOffset}");
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
                    PropertyDescription = 
                    "Your current time zone" +
                    $"Currently set to {User.TimeZoneOffset}",
                    Prompt =
                    "set [Time zone ID, for example 'Central Standard Time', or 'cst']",
                    Set = (string arg) =>
                    {
                        try
                        {
                            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(arg);
                            User.TimeZoneOffset = timeZone.BaseUtcOffset.TotalHours;
                            return true;
                        }
                        catch (TimeZoneNotFoundException)
                        {
                            // ignored, let's try looking for the named timezone instead
                        }

                        double offset = arg switch
                        {
                            "sst" => -11,
                            "ckt" or "hast" or "taht" => -10,
                            "akst" or "hadt" => -9,
                            "pst" or "akdt" => -8,
                            "mst" or "pdt" => -7,
                            "cst" or "mdt" => -6,
                            "cdt" or "est" => -5,
                            "clt" or "cost" or "ect" or "edt" => -4,
                            "brt" or "clst" => -3,
                            "uyst" => -2,
                            "brst" => -1,
                            "utc" or "gmt" => 0,
                            "bst" or "cet" or "ist" or "met" or "wat" => 1,
                            "cat" or "cest" or "eet" or "ist" or "sast" or "wast" => 2,
                            "ast" or "eat" or "eest" or "fet" or "idt" or "iot" or "msk" or "trt" => 3,
                            "irst" => 3.5,
                            "amt" or "azt" or "get" or "gst" or "mut" or "ret" or "samt" or "sct" or "volt" => 4,
                            "aft" or "irdt" => 4.5,
                            "mawt" or "mvt" or "orat" or "pkt" or "tft" or "tmt" or "uzt" or "yekt" => 5,
                            "ist" or "slst" => 5.5,
                            "npt" => 5.75,
                            "bst" or "btt" or "kgt" or "omst" or "vost" => 6,
                            "cct" or "mmt" => 6.5,
                            "cxt" or "davt" or "hovt" or "novt" or "ict" or "krat" or "tha" or "wit" => 7,
                            "awst" or "bdt" or "chot" or "cit" or "cst" or "hkt" or "irkt" or "mst" or "pht" or "sgt" or "wst" => 8,
                            "eit" or "jst" or "kst" or "yakt" => 9,
                            "acst" => 9.5,
                            "aest" or "chst" or "ddut" or "pgt" or "vlat" => 10,
                            "acdt" or "lhst" => 10.5,
                            "aedt" or "lhst" or "mist" or "nct" or "sbt" or "vut" => 11,
                            "fjt" or "mht" or "nzst" => 12,
                            "nzdt" or "tkt" => 13,
                            _ => 100 // magic number that will hopefully never be an offset
                        };
                        if (offset != 100)
                        {
                            User.TimeZoneOffset = offset;
                            return true;
                        }
                        else return false;
                    }
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
                $"**1** - Allow potential NSFW in non- NSFW channels: {Server.AllowsPotentialNSFW}\n" +
                $"**2** - Logging Channel: {Server.LoggingChannel}\n" +
                $"**3** - System Channel: {Server.SystemChannel}\n");
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
