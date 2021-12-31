// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.Net;
using Microsoft.Extensions.Logging;

namespace DisCatSharp
{
    /// <summary>
    /// Various Discord-related utilities.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Gets the version of the library
        /// </summary>
        internal static string VersionHeader { get; set; }

        /// <summary>
        /// Gets or sets the permission strings.
        /// </summary>
        internal static Dictionary<Permissions, string> PermissionStrings { get; set; }

        /// <summary>
        /// Gets the utf8 encoding
        /// </summary>
        internal static UTF8Encoding Utf8 { get; } = new UTF8Encoding(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="Utilities"/> class.
        /// </summary>
        static Utilities()
        {
            PermissionStrings = new Dictionary<Permissions, string>();
            var t = typeof(Permissions);
            var ti = t.GetTypeInfo();
            var vals = Enum.GetValues(t).Cast<Permissions>();

            foreach (var xv in vals)
            {
                var xsv = xv.ToString();
                var xmv = ti.DeclaredMembers.FirstOrDefault(Xm => Xm.Name == xsv);
                var xav = xmv.GetCustomAttribute<PermissionStringAttribute>();

                PermissionStrings[xv] = xav.String;
            }

            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var vs = "";
            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                vs = iv.InformationalVersion;
            else
            {
                var v = a.GetName().Version;
                vs = v.ToString(3);
            }

            VersionHeader = $"DiscordBot (https://github.com/Aiko-IT-Systems/DisCatSharp, v{vs})";
        }

        /// <summary>
        /// Gets the api base uri.
        /// </summary>
        /// <param name="Config">The config</param>
        /// <returns>A string.</returns>
        internal static string GetApiBaseUri(DiscordConfiguration Config = null)
            => Config == null ? Endpoints.BaseUri + "9" : Config.UseCanary ? Endpoints.CanaryUri + Config.ApiVersion : Endpoints.BaseUri + Config.ApiVersion;

        /// <summary>
        /// Gets the api uri for.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <param name="Config">The config</param>
        /// <returns>An Uri.</returns>
        internal static Uri GetApiUriFor(string Path, DiscordConfiguration Config)
            => new($"{GetApiBaseUri(Config)}{Path}");

        /// <summary>
        /// Gets the api uri for.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <param name="QueryString">The query string.</param>
        /// <param name="Config">The config</param>
        /// <returns>An Uri.</returns>
        internal static Uri GetApiUriFor(string Path, string QueryString, DiscordConfiguration Config)
            => new($"{GetApiBaseUri(Config)}{Path}{QueryString}");

        /// <summary>
        /// Gets the api uri builder for.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <param name="Config">The config</param>
        /// <returns>A QueryUriBuilder.</returns>
        internal static QueryUriBuilder GetApiUriBuilderFor(string Path, DiscordConfiguration Config)
            => new($"{GetApiBaseUri(Config)}{Path}");

        /// <summary>
        /// Gets the formatted token.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <returns>A string.</returns>
        internal static string GetFormattedToken(BaseDiscordClient Client) => GetFormattedToken(Client.Configuration);

        /// <summary>
        /// Gets the formatted token.
        /// </summary>
        /// <param name="Config">The config.</param>
        /// <returns>A string.</returns>
        internal static string GetFormattedToken(DiscordConfiguration Config)
        {
            return Config.TokenType switch
            {
                TokenType.Bearer => $"Bearer {Config.Token}",
                TokenType.Bot => $"Bot {Config.Token}",
                _ => throw new ArgumentException("Invalid token type specified.", nameof(Config.Token)),
            };
        }

        /// <summary>
        /// Gets the base headers.
        /// </summary>
        /// <returns>A Dictionary.</returns>
        internal static Dictionary<string, string> GetBaseHeaders()
            => new();

        /// <summary>
        /// Gets the user agent.
        /// </summary>
        /// <returns>A string.</returns>
        internal static string GetUserAgent()
            => VersionHeader;

        /// <summary>
        /// Contains the user mentions.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A bool.</returns>
        internal static bool ContainsUserMentions(string Message)
        {
            var pattern = @"<@(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(Message);
        }

        /// <summary>
        /// Contains the nickname mentions.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A bool.</returns>
        internal static bool ContainsNicknameMentions(string Message)
        {
            var pattern = @"<@!(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(Message);
        }

        /// <summary>
        /// Contains the channel mentions.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A bool.</returns>
        internal static bool ContainsChannelMentions(string Message)
        {
            var pattern = @"<#(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(Message);
        }

        /// <summary>
        /// Contains the role mentions.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A bool.</returns>
        internal static bool ContainsRoleMentions(string Message)
        {
            var pattern = @"<@&(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(Message);
        }

        /// <summary>
        /// Contains the emojis.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A bool.</returns>
        internal static bool ContainsEmojis(string Message)
        {
            var pattern = @"<a?:(.*):(\d+)>";
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            return regex.IsMatch(Message);
        }

        /// <summary>
        /// Gets the user mentions.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A list of ulong.</returns>
        internal static IEnumerable<ulong> GetUserMentions(DiscordMessage Message)
        {
            var regex = new Regex(@"<@!?(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(Message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the role mentions.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A list of ulong.</returns>
        internal static IEnumerable<ulong> GetRoleMentions(DiscordMessage Message)
        {
            var regex = new Regex(@"<@&(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(Message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the channel mentions.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A list of ulong.</returns>
        internal static IEnumerable<ulong> GetChannelMentions(DiscordMessage Message)
        {
            var regex = new Regex(@"<#(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(Message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the emojis.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>A list of ulong.</returns>
        internal static IEnumerable<ulong> GetEmojis(DiscordMessage Message)
        {
            var regex = new Regex(@"<a?:([a-zA-Z0-9_]+):(\d+)>", RegexOptions.ECMAScript);
            var matches = regex.Matches(Message.Content);
            foreach (Match match in matches)
                yield return ulong.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Are the valid slash command name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>A bool.</returns>
        internal static bool IsValidSlashCommandName(string Name)
        {
            var regex = new Regex(@"^[\w-]{1,32}$", RegexOptions.ECMAScript);
            return regex.IsMatch(Name);
        }

        /// <summary>
        /// Checks the thread auto archive duration feature.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="Taad">The taad.</param>
        /// <returns>A bool.</returns>
        internal static bool CheckThreadAutoArchiveDurationFeature(DiscordGuild Guild, ThreadAutoArchiveDuration Taad)
        {
            return Taad == ThreadAutoArchiveDuration.ThreeDays
                ? (Guild.PremiumTier.HasFlag(PremiumTier.TierOne) || Guild.Features.CanSetThreadArchiveDurationThreeDays)
                : Taad != ThreadAutoArchiveDuration.OneWeek || Guild.PremiumTier.HasFlag(PremiumTier.TierTwo) || Guild.Features.CanSetThreadArchiveDurationSevenDays;
        }

        /// <summary>
        /// Checks the thread private feature.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <returns>A bool.</returns>
        internal static bool CheckThreadPrivateFeature(DiscordGuild Guild) => Guild.PremiumTier.HasFlag(PremiumTier.TierTwo) || Guild.Features.CanCreatePrivateThreads;

        /// <summary>
        /// Have the message intents.
        /// </summary>
        /// <param name="Intents">The intents.</param>
        /// <returns>A bool.</returns>
        internal static bool HasMessageIntents(DiscordIntents Intents)
            => Intents.HasIntent(DiscordIntents.GuildMessages) || Intents.HasIntent(DiscordIntents.DirectMessages);

        /// <summary>
        /// Have the reaction intents.
        /// </summary>
        /// <param name="Intents">The intents.</param>
        /// <returns>A bool.</returns>
        internal static bool HasReactionIntents(DiscordIntents Intents)
            => Intents.HasIntent(DiscordIntents.GuildMessageReactions) || Intents.HasIntent(DiscordIntents.DirectMessageReactions);

        /// <summary>
        /// Have the typing intents.
        /// </summary>
        /// <param name="Intents">The intents.</param>
        /// <returns>A bool.</returns>
        internal static bool HasTypingIntents(DiscordIntents Intents)
            => Intents.HasIntent(DiscordIntents.GuildMessageTyping) || Intents.HasIntent(DiscordIntents.DirectMessageTyping);

        // https://discord.com/developers/docs/topics/gateway#sharding-sharding-formula
        /// <summary>
        /// Gets a shard id from a guild id and total shard count.
        /// </summary>
        /// <param name="GuildId">The guild id the shard is on.</param>
        /// <param name="ShardCount">The total amount of shards.</param>
        /// <returns>The shard id.</returns>
        public static int GetShardId(ulong GuildId, int ShardCount)
            => (int)(GuildId >> 22) % ShardCount;

        /// <summary>
        /// Helper method to create a <see cref="System.DateTimeOffset"/> from Unix time seconds for targets that do not support this natively.
        /// </summary>
        /// <param name="UnixTime">Unix time seconds to convert.</param>
        /// <param name="ShouldThrow">Whether the method should throw on failure. Defaults to true.</param>
        /// <returns>Calculated <see cref="System.DateTimeOffset"/>.</returns>
        public static DateTimeOffset GetDateTimeOffset(long UnixTime, bool ShouldThrow = true)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(UnixTime);
            }
            catch (Exception)
            {
                if (ShouldThrow)
                    throw;

                return DateTimeOffset.MinValue;
            }
        }

        /// <summary>
        /// Helper method to create a <see cref="System.DateTimeOffset"/> from Unix time milliseconds for targets that do not support this natively.
        /// </summary>
        /// <param name="UnixTime">Unix time milliseconds to convert.</param>
        /// <param name="ShouldThrow">Whether the method should throw on failure. Defaults to true.</param>
        /// <returns>Calculated <see cref="System.DateTimeOffset"/>.</returns>
        public static DateTimeOffset GetDateTimeOffsetFromMilliseconds(long UnixTime, bool ShouldThrow = true)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(UnixTime);
            }
            catch (Exception)
            {
                if (ShouldThrow)
                    throw;

                return DateTimeOffset.MinValue;
            }
        }

        /// <summary>
        /// Helper method to calculate Unix time seconds from a <see cref="System.DateTimeOffset"/> for targets that do not support this natively.
        /// </summary>
        /// <param name="Dto"><see cref="System.DateTimeOffset"/> to calculate Unix time for.</param>
        /// <returns>Calculated Unix time.</returns>
        public static long GetUnixTime(DateTimeOffset Dto)
            => Dto.ToUnixTimeMilliseconds();

        /// <summary>
        /// Computes a timestamp from a given snowflake.
        /// </summary>
        /// <param name="Snowflake">Snowflake to compute a timestamp from.</param>
        /// <returns>Computed timestamp.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset GetSnowflakeTime(this ulong Snowflake)
            => DiscordClient._discordEpoch.AddMilliseconds(Snowflake >> 22);

        /// <summary>
        /// Converts this <see cref="Permissions"/> into human-readable format.
        /// </summary>
        /// <param name="Perm">Permissions enumeration to convert.</param>
        /// <returns>Human-readable permissions.</returns>
        public static string ToPermissionString(this Permissions Perm)
        {
            if (Perm == Permissions.None)
                return PermissionStrings[Perm];

            Perm &= PermissionMethods.FullPerms;

            var strs = PermissionStrings
                .Where(Xkvp => Xkvp.Key != Permissions.None && (Perm & Xkvp.Key) == Xkvp.Key)
                .Select(Xkvp => Xkvp.Value);

            return string.Join(", ", strs.OrderBy(Xs => Xs));
        }

        /// <summary>
        /// Checks whether this string contains given characters.
        /// </summary>
        /// <param name="Str">String to check.</param>
        /// <param name="Characters">Characters to check for.</param>
        /// <returns>Whether the string contained these characters.</returns>
        public static bool Contains(this string Str, params char[] Characters)
        {
            foreach (var xc in Str)
                if (Characters.Contains(xc))
                    return true;

            return false;
        }

        /// <summary>
        /// Logs the task fault.
        /// </summary>
        /// <param name="Task">The task.</param>
        /// <param name="Logger">The logger.</param>
        /// <param name="Level">The level.</param>
        /// <param name="EventId">The event id.</param>
        /// <param name="Message">The message.</param>
        internal static void LogTaskFault(this Task Task, ILogger Logger, LogLevel Level, EventId EventId, string Message)
        {
            if (Task == null)
                throw new ArgumentNullException(nameof(Task));

            if (Logger == null)
                return;

            Task.ContinueWith(T => Logger.Log(Level, EventId, T.Exception, Message), TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Deconstructs the.
        /// </summary>
        /// <param name="Kvp">The kvp.</param>
        /// <param name="Key">The key.</param>
        /// <param name="Value">The value.</param>
        internal static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> Kvp, out TKey Key, out TValue Value)
        {
            Key = Kvp.Key;
            Value = Kvp.Value;
        }
    }
}
