// This file is part of the DisCatSharp project.
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters
{
    /// <summary>
    /// Represents a discord user converter.
    /// </summary>
    public class DiscordUserConverter : IArgumentConverter<DiscordUser>
    {
        /// <summary>
        /// Gets the user regex.
        /// </summary>
        private static Regex UserRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordUserConverter"/> class.
        /// </summary>
        static DiscordUserConverter()
        {
#if NETSTANDARD1_3
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript);
#else
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        async Task<Optional<DiscordUser>> IArgumentConverter<DiscordUser>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await ctx.Client.GetUserAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await ctx.Client.GetUserAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Client.Guilds.Values
                .SelectMany(xkvp => xkvp.Members.Values)
                .Where(xm => (cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null));

            var usr = us.FirstOrDefault();
            return usr != null ? Optional.FromValue<DiscordUser>(usr) : Optional.FromNoValue<DiscordUser>();
        }
    }

    /// <summary>
    /// Represents a discord member converter.
    /// </summary>
    public class DiscordMemberConverter : IArgumentConverter<DiscordMember>
    {
        /// <summary>
        /// Gets the user regex.
        /// </summary>
        private static Regex UserRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMemberConverter"/> class.
        /// </summary>
        static DiscordMemberConverter()
        {
#if NETSTANDARD1_3
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript);
#else
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        async Task<Optional<DiscordMember>> IArgumentConverter<DiscordMember>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild == null)
                return Optional.FromNoValue<DiscordMember>();

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            var searchResult = await ctx.Guild.SearchMembersAsync(value);
            if (searchResult.Any())
                return Optional.FromValue(searchResult.First());

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Guild.Members.Values
                .Where(xm => ((cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null))
                          || (cs ? xm.Nickname : xm.Nickname?.ToLowerInvariant()) == value);

            var mbr = us.FirstOrDefault();
            return mbr != null ? Optional.FromValue(mbr) : Optional.FromNoValue<DiscordMember>();
        }
    }

    /// <summary>
    /// Represents a discord channel converter.
    /// </summary>
    public class DiscordChannelConverter : IArgumentConverter<DiscordChannel>
    {
        /// <summary>
        /// Gets the channel regex.
        /// </summary>
        private static Regex ChannelRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordChannelConverter"/> class.
        /// </summary>
        static DiscordChannelConverter()
        {
#if NETSTANDARD1_3
            ChannelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript);
#else
            ChannelRegex = new Regex(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        async Task<Optional<DiscordChannel>> IArgumentConverter<DiscordChannel>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid))
            {
                var result = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
                return ret;
            }

            var m = ChannelRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
            {
                var result = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
                return ret;
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var chn = ctx.Guild?.Channels.Values.FirstOrDefault(xc => (cs ? xc.Name : xc.Name.ToLowerInvariant()) == value);
            return chn != null ? Optional.FromValue(chn) : Optional.FromNoValue<DiscordChannel>();
        }
    }

    /// <summary>
    /// Represents a discord role converter.
    /// </summary>
    public class DiscordRoleConverter : IArgumentConverter<DiscordRole>
    {
        /// <summary>
        /// Gets the role regex.
        /// </summary>
        private static Regex RoleRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRoleConverter"/> class.
        /// </summary>
        static DiscordRoleConverter()
        {
#if NETSTANDARD1_3
            RoleRegex = new Regex(@"^<@&(\d+?)>$", RegexOptions.ECMAScript);
#else
            RoleRegex = new Regex(@"^<@&(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        Task<Optional<DiscordRole>> IArgumentConverter<DiscordRole>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild == null)
                return Task.FromResult(Optional.FromNoValue<DiscordRole>());

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rid))
            {
                var result = ctx.Guild.GetRole(rid);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
                return Task.FromResult(ret);
            }

            var m = RoleRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out rid))
            {
                var result = ctx.Guild.GetRole(rid);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
                return Task.FromResult(ret);
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value.ToLowerInvariant();

            var rol = ctx.Guild.Roles.Values.FirstOrDefault(xr => (cs ? xr.Name : xr.Name.ToLowerInvariant()) == value);
            return Task.FromResult(rol != null ? Optional.FromValue(rol) : Optional.FromNoValue<DiscordRole>());
        }
    }

    /// <summary>
    /// Represents a discord guild converter.
    /// </summary>
    public class DiscordGuildConverter : IArgumentConverter<DiscordGuild>
    {
        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        Task<Optional<DiscordGuild>> IArgumentConverter<DiscordGuild>.ConvertAsync(string value, CommandContext ctx)
        {
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gid))
            {
                return ctx.Client.Guilds.TryGetValue(gid, out var result)
                    ? Task.FromResult(Optional.FromValue(result))
                    : Task.FromResult(Optional.FromNoValue<DiscordGuild>());
            }

            var cs = ctx.Config.CaseSensitive;
            if (!cs)
                value = value?.ToLowerInvariant();

            var gld = ctx.Client.Guilds.Values.FirstOrDefault(xg => (cs ? xg.Name : xg.Name.ToLowerInvariant()) == value);
            return Task.FromResult(gld != null ? Optional.FromValue(gld) : Optional.FromNoValue<DiscordGuild>());
        }
    }

    /// <summary>
    /// Represents a discord message converter.
    /// </summary>
    public class DiscordMessageConverter : IArgumentConverter<DiscordMessage>
    {
        /// <summary>
        /// Gets the message path regex.
        /// </summary>
        private static Regex MessagePathRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMessageConverter"/> class.
        /// </summary>
        static DiscordMessageConverter()
        {
#if NETSTANDARD1_3
            MessagePathRegex = new Regex(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$", RegexOptions.ECMAScript);
#else
            MessagePathRegex = new Regex(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        async Task<Optional<DiscordMessage>> IArgumentConverter<DiscordMessage>.ConvertAsync(string value, CommandContext ctx)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Optional.FromNoValue<DiscordMessage>();

            var msguri = value.StartsWith("<") && value.EndsWith(">") ? value.Substring(1, value.Length - 2) : value;
            ulong mid;
            if (Uri.TryCreate(msguri, UriKind.Absolute, out var uri))
            {
                if (uri.Host != "discordapp.com" && uri.Host != "discord.com" && !uri.Host.EndsWith(".discordapp.com") && !uri.Host.EndsWith(".discord.com"))
                    return Optional.FromNoValue<DiscordMessage>();

                var uripath = MessagePathRegex.Match(uri.AbsolutePath);
                if (!uripath.Success
                    || !ulong.TryParse(uripath.Groups["channel"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid)
                    || !ulong.TryParse(uripath.Groups["message"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
                    return Optional.FromNoValue<DiscordMessage>();

                var chn = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                if (chn == null)
                    return Optional.FromNoValue<DiscordMessage>();

                var msg = await chn.GetMessageAsync(mid).ConfigureAwait(false);
                return msg != null ? Optional.FromValue(msg) : Optional.FromNoValue<DiscordMessage>();
            }

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
            {
                var result = await ctx.Channel.GetMessageAsync(mid).ConfigureAwait(false);
                return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMessage>();
            }

            return Optional.FromNoValue<DiscordMessage>();
        }
    }

    /// <summary>
    /// Represents a discord emoji converter.
    /// </summary>
    public class DiscordEmojiConverter : IArgumentConverter<DiscordEmoji>
    {
        /// <summary>
        /// Gets the emote regex.
        /// </summary>
        private static Regex EmoteRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordEmojiConverter"/> class.
        /// </summary>
        static DiscordEmojiConverter()
        {
#if NETSTANDARD1_3
            EmoteRegex = new Regex(@"^<a?:([a-zA-Z0-9_]+?):(\d+?)>$", RegexOptions.ECMAScript);
#else
            EmoteRegex = new Regex(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        Task<Optional<DiscordEmoji>> IArgumentConverter<DiscordEmoji>.ConvertAsync(string value, CommandContext ctx)
        {
            if (DiscordEmoji.TryFromUnicode(ctx.Client, value, out var emoji))
            {
                var result = emoji;
                var ret = Optional.FromValue(result);
                return Task.FromResult(ret);
            }

            var m = EmoteRegex.Match(value);
            if (m.Success)
            {
                var sid = m.Groups["id"].Value;
                var name = m.Groups["name"].Value;
                var anim = m.Groups["animated"].Success;

                return !ulong.TryParse(sid, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                    ? Task.FromResult(Optional.FromNoValue<DiscordEmoji>())
                    : DiscordEmoji.TryFromGuildEmote(ctx.Client, id, out emoji)
                    ? Task.FromResult(Optional.FromValue(emoji))
                    : Task.FromResult(Optional.FromValue(new DiscordEmoji
                {
                    Discord = ctx.Client,
                    Id = id,
                    Name = name,
                    IsAnimated = anim,
                    RequiresColons = true,
                    IsManaged = false
                }));
            }

            return Task.FromResult(Optional.FromNoValue<DiscordEmoji>());
        }
    }

    /// <summary>
    /// Represents a discord color converter.
    /// </summary>
    public class DiscordColorConverter : IArgumentConverter<DiscordColor>
    {
        /// <summary>
        /// Gets the color regex hex.
        /// </summary>
        private static Regex ColorRegexHex { get; }
        /// <summary>
        /// Gets the color regex rgb.
        /// </summary>
        private static Regex ColorRegexRgb { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordColorConverter"/> class.
        /// </summary>
        static DiscordColorConverter()
        {
#if NETSTANDARD1_3
            ColorRegexHex = new Regex(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript);
            ColorRegexRgb = new Regex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript);
#else
            ColorRegexHex = new Regex(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
            ColorRegexRgb = new Regex(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        Task<Optional<DiscordColor>> IArgumentConverter<DiscordColor>.ConvertAsync(string value, CommandContext ctx)
        {
            var m = ColorRegexHex.Match(value);
            if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var clr))
                return Task.FromResult(Optional.FromValue<DiscordColor>(clr));

            m = ColorRegexRgb.Match(value);
            if (m.Success)
            {
                var p1 = byte.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r);
                var p2 = byte.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var g);
                var p3 = byte.TryParse(m.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b);

                return !(p1 && p2 && p3)
                    ? Task.FromResult(Optional.FromNoValue<DiscordColor>())
                    : Task.FromResult(Optional.FromValue(new DiscordColor(r, g, b)));
            }

            return Task.FromResult(Optional.FromNoValue<DiscordColor>());
        }
    }
}
