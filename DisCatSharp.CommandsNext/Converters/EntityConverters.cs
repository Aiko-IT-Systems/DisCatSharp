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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisCatSharp.Common.RegularExpressions;
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
            UserRegex = DiscordRegEx.User;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<DiscordUser>> IArgumentConverter<DiscordUser>.Convert(string Value, CommandContext Ctx)
        {
            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await Ctx.Client.GetUserAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            var m = UserRegex.Match(Value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await Ctx.Client.GetUserAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            var cs = Ctx.Config.CaseSensitive;
            if (!cs)
                Value = Value.ToLowerInvariant();

            var di = Value.IndexOf('#');
            var un = di != -1 ? Value[..di] : Value;
            var dv = di != -1 ? Value[(di + 1)..] : null;

            var us = Ctx.Client.Guilds.Values
                .SelectMany(Xkvp => Xkvp.Members.Values)
                .Where(Xm => (cs ? Xm.Username : Xm.Username.ToLowerInvariant()) == un && ((dv != null && Xm.Discriminator == dv) || dv == null));

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
            UserRegex = DiscordRegEx.User;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<DiscordMember>> IArgumentConverter<DiscordMember>.Convert(string Value, CommandContext Ctx)
        {
            if (Ctx.Guild == null)
                return Optional.FromNoValue<DiscordMember>();

            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await Ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            var m = UserRegex.Match(Value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await Ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            var searchResult = await Ctx.Guild.SearchMembers(Value).ConfigureAwait(false);
            if (searchResult.Any())
                return Optional.FromValue(searchResult.First());

            var cs = Ctx.Config.CaseSensitive;
            if (!cs)
                Value = Value.ToLowerInvariant();

            var di = Value.IndexOf('#');
            var un = di != -1 ? Value[..di] : Value;
            var dv = di != -1 ? Value[(di + 1)..] : null;

            var us = Ctx.Guild.Members.Values
                .Where(Xm => ((cs ? Xm.Username : Xm.Username.ToLowerInvariant()) == un && ((dv != null && Xm.Discriminator == dv) || dv == null))
                          || (cs ? Xm.Nickname : Xm.Nickname?.ToLowerInvariant()) == Value);

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
            ChannelRegex = DiscordRegEx.Channel;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<DiscordChannel>> IArgumentConverter<DiscordChannel>.Convert(string Value, CommandContext Ctx)
        {
            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid))
            {
                var result = await Ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
                return ret;
            }

            var m = ChannelRegex.Match(Value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
            {
                var result = await Ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordChannel>();
                return ret;
            }

            var cs = Ctx.Config.CaseSensitive;
            if (!cs)
                Value = Value.ToLowerInvariant();

            var chn = Ctx.Guild?.Channels.Values.FirstOrDefault(Xc => (cs ? Xc.Name : Xc.Name.ToLowerInvariant()) == Value);
            return chn != null ? Optional.FromValue(chn) : Optional.FromNoValue<DiscordChannel>();
        }
    }

    /// <summary>
    /// Represents a discord thread channel converter.
    /// </summary>
    public class DiscordThreadChannelConverter : IArgumentConverter<DiscordThreadChannel>
    {
        /// <summary>
        /// Gets the channel regex.
        /// </summary>
        private static Regex ChannelRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordThreadChannelConverter"/> class.
        /// </summary>
        static DiscordThreadChannelConverter()
        {
            ChannelRegex = DiscordRegEx.Channel;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<DiscordThreadChannel>> IArgumentConverter<DiscordThreadChannel>.Convert(string Value, CommandContext Ctx)
        {
            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tid))
            {
                var result = await Ctx.Client.GetThreadAsync(tid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordThreadChannel>();
                return ret;
            }

            var m = ChannelRegex.Match(Value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out tid))
            {
                var result = await Ctx.Client.GetThreadAsync(tid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordThreadChannel>();
                return ret;
            }

            var cs = Ctx.Config.CaseSensitive;
            if (!cs)
                Value = Value.ToLowerInvariant();

            var tchn = Ctx.Guild?.Threads.Values.FirstOrDefault(Xc => (cs ? Xc.Name : Xc.Name.ToLowerInvariant()) == Value);
            return tchn != null ? Optional.FromValue(tchn) : Optional.FromNoValue<DiscordThreadChannel>();
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
            RoleRegex = DiscordRegEx.Role;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        Task<Optional<DiscordRole>> IArgumentConverter<DiscordRole>.Convert(string Value, CommandContext Ctx)
        {
            if (Ctx.Guild == null)
                return Task.FromResult(Optional.FromNoValue<DiscordRole>());

            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rid))
            {
                var result = Ctx.Guild.GetRole(rid);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
                return Task.FromResult(ret);
            }

            var m = RoleRegex.Match(Value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out rid))
            {
                var result = Ctx.Guild.GetRole(rid);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordRole>();
                return Task.FromResult(ret);
            }

            var cs = Ctx.Config.CaseSensitive;
            if (!cs)
                Value = Value.ToLowerInvariant();

            var rol = Ctx.Guild.Roles.Values.FirstOrDefault(Xr => (cs ? Xr.Name : Xr.Name.ToLowerInvariant()) == Value);
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
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        Task<Optional<DiscordGuild>> IArgumentConverter<DiscordGuild>.Convert(string Value, CommandContext Ctx)
        {
            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gid))
            {
                return Ctx.Client.Guilds.TryGetValue(gid, out var result)
                    ? Task.FromResult(Optional.FromValue(result))
                    : Task.FromResult(Optional.FromNoValue<DiscordGuild>());
            }

            var cs = Ctx.Config.CaseSensitive;
            if (!cs)
                Value = Value?.ToLowerInvariant();

            var gld = Ctx.Client.Guilds.Values.FirstOrDefault(Xg => (cs ? Xg.Name : Xg.Name.ToLowerInvariant()) == Value);
            return Task.FromResult(gld != null ? Optional.FromValue(gld) : Optional.FromNoValue<DiscordGuild>());
        }
    }


    /// <summary>
    /// Represents a discord invite converter.
    /// </summary>
    public class DiscordInviteConverter : IArgumentConverter<DiscordInvite>
    {
        /// <summary>
        /// Gets the invite regex.
        /// </summary>
        private static Regex InviteRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordInviteConverter"/> class.
        /// </summary>
        static DiscordInviteConverter()
        {
            InviteRegex = DiscordRegEx.Invite;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<DiscordInvite>> IArgumentConverter<DiscordInvite>.Convert(string Value, CommandContext Ctx)
        {
            var m = InviteRegex.Match(Value);
            if (m.Success)
            {
                var result = await Ctx.Client.GetInviteByCode(m.Groups[5].Value).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordInvite>();
                return ret;
            }

            var cs = Ctx.Config.CaseSensitive;
            if (!cs)
                Value = Value?.ToLowerInvariant();

            var inv  = await Ctx.Client.GetInviteByCode(Value);
            return inv != null ? Optional.FromValue(inv) : Optional.FromNoValue<DiscordInvite>();
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
            MessagePathRegex = DiscordRegEx.MessageLink;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<DiscordMessage>> IArgumentConverter<DiscordMessage>.Convert(string Value, CommandContext Ctx)
        {
            if (string.IsNullOrWhiteSpace(Value))
                return Optional.FromNoValue<DiscordMessage>();

            var msguri = Value.StartsWith("<") && Value.EndsWith(">") ? Value[1..^1] : Value;
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

                var chn = await Ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
                if (chn == null)
                    return Optional.FromNoValue<DiscordMessage>();

                var msg = await chn.GetMessageAsync(mid).ConfigureAwait(false);
                return msg != null ? Optional.FromValue(msg) : Optional.FromNoValue<DiscordMessage>();
            }

            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
            {
                var result = await Ctx.Channel.GetMessageAsync(mid).ConfigureAwait(false);
                return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMessage>();
            }

            return Optional.FromNoValue<DiscordMessage>();
        }
    }

    /// <summary>
    /// Represents a discord scheduled event converter.
    /// </summary>
    public class DiscordScheduledEventConverter : IArgumentConverter<DiscordScheduledEvent>
    {
        /// <summary>
        /// Gets the event regex.
        /// </summary>
        private static Regex EventRegex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordScheduledEventConverter"/> class.
        /// </summary>
        static DiscordScheduledEventConverter()
        {
            EventRegex = DiscordRegEx.Event;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<DiscordScheduledEvent>> IArgumentConverter<DiscordScheduledEvent>.Convert(string Value, CommandContext Ctx)
        {
            if (string.IsNullOrWhiteSpace(Value))
                return Optional.FromNoValue<DiscordScheduledEvent>();

            var msguri = Value.StartsWith("<") && Value.EndsWith(">") ? Value[1..^1] : Value;
            ulong seid;
            if (Uri.TryCreate(msguri, UriKind.Absolute, out var uri))
            {
                if (uri.Host != "discordapp.com" && uri.Host != "discord.com" && !uri.Host.EndsWith(".discordapp.com") && !uri.Host.EndsWith(".discord.com"))
                    return Optional.FromNoValue<DiscordScheduledEvent>();

                var uripath = EventRegex.Match(uri.AbsolutePath);
                if (!uripath.Success
                    || !ulong.TryParse(uripath.Groups["guild"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gid)
                    || !ulong.TryParse(uripath.Groups["event"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seid))
                    return Optional.FromNoValue<DiscordScheduledEvent>();

                var guild = await Ctx.Client.GetGuildAsync(gid).ConfigureAwait(false);
                if (guild == null)
                    return Optional.FromNoValue<DiscordScheduledEvent>();

                var ev = await guild.GetScheduledEventAsync(seid).ConfigureAwait(false);
                return ev != null ? Optional.FromValue(ev) : Optional.FromNoValue<DiscordScheduledEvent>();
            }

            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seid))
            {
                var result = await Ctx.Guild.GetScheduledEventAsync(seid).ConfigureAwait(false);
                return result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordScheduledEvent>();
            }

            return Optional.FromNoValue<DiscordScheduledEvent>();
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
            EmoteRegex = DiscordRegEx.Emoji;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        Task<Optional<DiscordEmoji>> IArgumentConverter<DiscordEmoji>.Convert(string Value, CommandContext Ctx)
        {
            if (DiscordEmoji.TryFromUnicode(Ctx.Client, Value, out var emoji))
            {
                var result = emoji;
                var ret = Optional.FromValue(result);
                return Task.FromResult(ret);
            }

            var m = EmoteRegex.Match(Value);
            if (m.Success)
            {
                var sid = m.Groups["id"].Value;
                var name = m.Groups["name"].Value;
                var anim = m.Groups["animated"].Success;

                return !ulong.TryParse(sid, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                    ? Task.FromResult(Optional.FromNoValue<DiscordEmoji>())
                    : DiscordEmoji.TryFromGuildEmote(Ctx.Client, id, out emoji)
                    ? Task.FromResult(Optional.FromValue(emoji))
                    : Task.FromResult(Optional.FromValue(new DiscordEmoji
                    {
                        Discord = Ctx.Client,
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
            ColorRegexHex = CommonRegEx.HexColorString;
            ColorRegexRgb = CommonRegEx.RgbColorString;
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        Task<Optional<DiscordColor>> IArgumentConverter<DiscordColor>.Convert(string Value, CommandContext Ctx)
        {
            var m = ColorRegexHex.Match(Value);
            if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var clr))
                return Task.FromResult(Optional.FromValue<DiscordColor>(clr));

            m = ColorRegexRgb.Match(Value);
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
