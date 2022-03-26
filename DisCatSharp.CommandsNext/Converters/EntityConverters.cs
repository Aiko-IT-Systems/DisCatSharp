// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
		private static Regex s_userRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordUserConverter"/> class.
		/// </summary>
		static DiscordUserConverter()
		{
			s_userRegex = DiscordRegEx.User;
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
				return Optional.FromNullable(result);
			}

			var m = s_userRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
			{
				var result = await ctx.Client.GetUserAsync(uid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			var cs = ctx.Config.CaseSensitive;
			if (!cs)
				value = value.ToLowerInvariant();

			var di = value.IndexOf('#');
			var un = di != -1 ? value[..di] : value;
			var dv = di != -1 ? value[(di + 1)..] : null;

			var us = ctx.Client.Guilds.Values
				.SelectMany(xkvp => xkvp.Members.Values)
				.Where(xm => (cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null));

			var usr = us.FirstOrDefault();
			return Optional.FromNullable<DiscordUser>(usr);
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
		private static Regex s_userRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordMemberConverter"/> class.
		/// </summary>
		static DiscordMemberConverter()
		{
			s_userRegex = DiscordRegEx.User;
		}

		/// <summary>
		/// Converts a string.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="ctx">The command context.</param>
		async Task<Optional<DiscordMember>> IArgumentConverter<DiscordMember>.ConvertAsync(string value, CommandContext ctx)
		{
			if (ctx.Guild == null)
				return Optional.None;

			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
			{
				var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			var m = s_userRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
			{
				var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			var searchResult = await ctx.Guild.SearchMembersAsync(value).ConfigureAwait(false);
			if (searchResult.Any())
				return Optional.Some(searchResult.First());

			var cs = ctx.Config.CaseSensitive;
			if (!cs)
				value = value.ToLowerInvariant();

			var di = value.IndexOf('#');
			var un = di != -1 ? value[..di] : value;
			var dv = di != -1 ? value[(di + 1)..] : null;

			var us = ctx.Guild.Members.Values
				.Where(xm => ((cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null))
						  || (cs ? xm.Nickname : xm.Nickname?.ToLowerInvariant()) == value);

			return Optional.FromNullable(us.FirstOrDefault());
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
		private static Regex s_channelRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordChannelConverter"/> class.
		/// </summary>
		static DiscordChannelConverter()
		{
			s_channelRegex = DiscordRegEx.Channel;
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
				return Optional.FromNullable(result);
			}

			var m = s_channelRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cid))
			{
				var result = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			var cs = ctx.Config.CaseSensitive;
			if (!cs)
				value = value.ToLowerInvariant();

			var chn = ctx.Guild?.Channels.Values.FirstOrDefault(xc => (cs ? xc.Name : xc.Name.ToLowerInvariant()) == value);
			return Optional.FromNullable(chn);
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
		private static Regex s_channelRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordThreadChannelConverter"/> class.
		/// </summary>
		static DiscordThreadChannelConverter()
		{
			s_channelRegex = DiscordRegEx.Channel;
		}

		/// <summary>
		/// Converts a string.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="ctx">The command context.</param>
		async Task<Optional<DiscordThreadChannel>> IArgumentConverter<DiscordThreadChannel>.ConvertAsync(string value, CommandContext ctx)
		{
			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tid))
			{
				var result = await ctx.Client.GetThreadAsync(tid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			var m = s_channelRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out tid))
			{
				var result = await ctx.Client.GetThreadAsync(tid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			var cs = ctx.Config.CaseSensitive;
			if (!cs)
				value = value.ToLowerInvariant();

			var tchn = ctx.Guild?.Threads.Values.FirstOrDefault(xc => (cs ? xc.Name : xc.Name.ToLowerInvariant()) == value);
			return Optional.FromNullable(tchn);
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
		private static Regex s_roleRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordRoleConverter"/> class.
		/// </summary>
		static DiscordRoleConverter()
		{
			s_roleRegex = DiscordRegEx.Role;
		}

		/// <summary>
		/// Converts a string.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="ctx">The command context.</param>
		Task<Optional<DiscordRole>> IArgumentConverter<DiscordRole>.ConvertAsync(string value, CommandContext ctx)
		{
			if (ctx.Guild == null)
				return Task.FromResult(Optional<DiscordRole>.None);

			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rid))
			{
				var result = ctx.Guild.GetRole(rid);
				return Task.FromResult(Optional.FromNullable(result));
			}

			var m = s_roleRegex.Match(value);
			if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out rid))
			{
				var result = ctx.Guild.GetRole(rid);
				return Task.FromResult(Optional.FromNullable(result));
			}

			var cs = ctx.Config.CaseSensitive;
			if (!cs)
				value = value.ToLowerInvariant();

			var rol = ctx.Guild.Roles.Values.FirstOrDefault(xr => (cs ? xr.Name : xr.Name.ToLowerInvariant()) == value);
			return Task.FromResult(Optional.FromNullable(rol));
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
					? Task.FromResult(Optional.Some(result))
					: Task.FromResult(Optional<DiscordGuild>.None);
			}

			var cs = ctx.Config.CaseSensitive;
			if (!cs)
				value = value?.ToLowerInvariant();

			var gld = ctx.Client.Guilds.Values.FirstOrDefault(xg => (cs ? xg.Name : xg.Name.ToLowerInvariant()) == value);
			return Task.FromResult(Optional.FromNullable(gld));
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
		private static Regex s_inviteRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordInviteConverter"/> class.
		/// </summary>
		static DiscordInviteConverter()
		{
			s_inviteRegex = DiscordRegEx.Invite;
		}

		/// <summary>
		/// Converts a string.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="ctx">The command context.</param>
		async Task<Optional<DiscordInvite>> IArgumentConverter<DiscordInvite>.ConvertAsync(string value, CommandContext ctx)
		{
			var m = s_inviteRegex.Match(value);
			if (m.Success)
			{
				var result = await ctx.Client.GetInviteByCodeAsync(m.Groups[5].Value).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			var cs = ctx.Config.CaseSensitive;
			if (!cs)
				value = value?.ToLowerInvariant();

			var inv = await ctx.Client.GetInviteByCodeAsync(value);
			return Optional.FromNullable(inv);
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
		private static Regex s_messagePathRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordMessageConverter"/> class.
		/// </summary>
		static DiscordMessageConverter()
		{
			s_messagePathRegex = DiscordRegEx.MessageLink;
		}

		/// <summary>
		/// Converts a string.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="ctx">The command context.</param>
		async Task<Optional<DiscordMessage>> IArgumentConverter<DiscordMessage>.ConvertAsync(string value, CommandContext ctx)
		{
			if (string.IsNullOrWhiteSpace(value))
				return Optional.None;

			var msguri = value.StartsWith("<") && value.EndsWith(">") ? value[1..^1] : value;
			ulong mid;
			if (Uri.TryCreate(msguri, UriKind.Absolute, out var uri))
			{
				if (uri.Host != "discordapp.com" && uri.Host != "discord.com" && !uri.Host.EndsWith(".discordapp.com") && !uri.Host.EndsWith(".discord.com"))
					return Optional.None;

				var uripath = s_messagePathRegex.Match(uri.AbsolutePath);
				if (!uripath.Success
					|| !ulong.TryParse(uripath.Groups["channel"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cid)
					|| !ulong.TryParse(uripath.Groups["message"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
					return Optional.None;

				var chn = await ctx.Client.GetChannelAsync(cid).ConfigureAwait(false);
				if (chn == null)
					return Optional.None;

				var msg = await chn.GetMessageAsync(mid).ConfigureAwait(false);
				return Optional.FromNullable(msg);
			}

			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out mid))
			{
				var result = await ctx.Channel.GetMessageAsync(mid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			return Optional.None;
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
		private static Regex s_eventRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordScheduledEventConverter"/> class.
		/// </summary>
		static DiscordScheduledEventConverter()
		{
			s_eventRegex = DiscordRegEx.Event;
		}

		/// <summary>
		/// Converts a string.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="ctx">The command context.</param>
		async Task<Optional<DiscordScheduledEvent>> IArgumentConverter<DiscordScheduledEvent>.ConvertAsync(string value, CommandContext ctx)
		{
			if (string.IsNullOrWhiteSpace(value))
				return Optional.None;

			var msguri = value.StartsWith("<") && value.EndsWith(">") ? value[1..^1] : value;
			ulong seid;
			if (Uri.TryCreate(msguri, UriKind.Absolute, out var uri))
			{
				if (uri.Host != "discordapp.com" && uri.Host != "discord.com" && !uri.Host.EndsWith(".discordapp.com") && !uri.Host.EndsWith(".discord.com"))
					return Optional.None;

				var uripath = s_eventRegex.Match(uri.AbsolutePath);
				if (!uripath.Success
					|| !ulong.TryParse(uripath.Groups["guild"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gid)
					|| !ulong.TryParse(uripath.Groups["event"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seid))
					return Optional.None;

				var guild = await ctx.Client.GetGuildAsync(gid).ConfigureAwait(false);
				if (guild == null)
					return Optional.None;

				var ev = await guild.GetScheduledEventAsync(seid).ConfigureAwait(false);
				return Optional.FromNullable(ev);
			}

			if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out seid))
			{
				var result = await ctx.Guild.GetScheduledEventAsync(seid).ConfigureAwait(false);
				return Optional.FromNullable(result);
			}

			return Optional.None;
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
		private static Regex s_emoteRegex { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordEmojiConverter"/> class.
		/// </summary>
		static DiscordEmojiConverter()
		{
			s_emoteRegex = DiscordRegEx.Emoji;
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
				return Task.FromResult(Optional.Some(result));
			}

			var m = s_emoteRegex.Match(value);
			if (m.Success)
			{
				var sid = m.Groups["id"].Value;
				var name = m.Groups["name"].Value;
				var anim = m.Groups["animated"].Success;

				return !ulong.TryParse(sid, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
					? Task.FromResult(Optional<DiscordEmoji>.None)
					: DiscordEmoji.TryFromGuildEmote(ctx.Client, id, out emoji)
					? Task.FromResult(Optional.Some(emoji))
					: Task.FromResult(Optional.Some(new DiscordEmoji
					{
						Discord = ctx.Client,
						Id = id,
						Name = name,
						IsAnimated = anim,
						RequiresColons = true,
						IsManaged = false
					}));
			}

			return Task.FromResult(Optional<DiscordEmoji>.None);
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
		private static Regex s_colorRegexHex { get; }
		/// <summary>
		/// Gets the color regex rgb.
		/// </summary>
		private static Regex s_colorRegexRgb { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordColorConverter"/> class.
		/// </summary>
		static DiscordColorConverter()
		{
			s_colorRegexHex = CommonRegEx.HexColorString;
			s_colorRegexRgb = CommonRegEx.RgbColorString;
		}

		/// <summary>
		/// Converts a string.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="ctx">The command context.</param>
		Task<Optional<DiscordColor>> IArgumentConverter<DiscordColor>.ConvertAsync(string value, CommandContext ctx)
		{
			var m = s_colorRegexHex.Match(value);
			if (m.Success && int.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var clr))
				return Task.FromResult(Optional.Some<DiscordColor>(clr));

			m = s_colorRegexRgb.Match(value);
			if (m.Success)
			{
				var p1 = byte.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r);
				var p2 = byte.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var g);
				var p3 = byte.TryParse(m.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b);

				return !(p1 && p2 && p3)
					? Task.FromResult(Optional<DiscordColor>.None)
					: Task.FromResult(Optional.Some(new DiscordColor(r, g, b)));
			}

			return Task.FromResult(Optional<DiscordColor>.None);
		}
	}
}
