// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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
using System.Text.RegularExpressions;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
/// Contains markdown formatting helpers.
/// </summary>
public static class Formatter
{
	/// <summary>
	/// Gets the md sanitize regex.
	/// </summary>
	private static Regex s_mdSanitizeRegex { get; } = new(@"([`\*_~<>\[\]\(\)""@\!\&#:\|])", RegexOptions.ECMAScript);
	/// <summary>
	/// Gets the md strip regex.
	/// </summary>
	private static Regex s_mdStripRegex { get; } = new(@"([`\*_~\[\]\(\)""\|]|<@\!?\d+>|<#\d+>|<@\&\d+>|<:[a-zA-Z0-9_\-]:\d+>)", RegexOptions.ECMAScript);

	/// <summary>
	/// Creates a block of code.
	/// </summary>
	/// <param name="content">Contents of the block.</param>
	/// <param name="language">Language to use for highlighting.</param>
	/// <returns>Formatted block of code.</returns>
	public static string BlockCode(this string content, string language = "")
		=> $"```{language}\n{content}\n```";

	/// <summary>
	/// Creates inline code snippet.
	/// </summary>
	/// <param name="content">Contents of the snippet.</param>
	/// <returns>Formatted inline code snippet.</returns>
	public static string InlineCode(this string content)
		=> $"`{content}`";

	/// <summary>
	/// Creates a rendered timestamp.
	/// </summary>
	/// <param name="time">The time from now.</param>
	/// <param name="format">The format to render the timestamp in. Defaults to relative.</param>
	/// <returns>A formatted timestamp.</returns>
	public static string Timestamp(this TimeSpan time, TimestampFormat format = TimestampFormat.RelativeTime)
		=> Timestamp(DateTimeOffset.UtcNow + time, format);

	/// <summary>
	/// Creates a rendered timestamp.
	/// </summary>
	/// <param name="time">Timestamp to format.</param>
	/// <param name="format">The format to render the timestamp in. Defaults to relative.</param>
	/// <returns>A formatted timestamp.</returns>
	public static string Timestamp(this DateTimeOffset time, TimestampFormat format = TimestampFormat.RelativeTime)
		=> $"<t:{time.ToUnixTimeSeconds()}:{(char)format}>";

	/// <summary>
	/// Creates a rendered timestamp.
	/// </summary>
	/// <param name="time">The time from now.</param>
	/// <param name="format">The format to render the timestamp in. Defaults to relative.</param>
	/// <returns>A formatted timestamp relative to now.</returns>
	public static string Timestamp(this DateTime time, TimestampFormat format = TimestampFormat.RelativeTime)
		=> Timestamp(time.ToUniversalTime() - DateTime.UtcNow, format);

	/// <summary>
	/// Creates bold text.
	/// </summary>
	/// <param name="content">Text to embolden.</param>
	/// <returns>Formatted text.</returns>
	public static string Bold(this string content)
		=> $"**{content}**";

	/// <summary>
	/// Creates italicized text.
	/// </summary>
	/// <param name="content">Text to italicize.</param>
	/// <returns>Formatted text.</returns>
	public static string Italic(this string content)
		=> $"*{content}*";

	/// <summary>
	/// Creates spoiler from text.
	/// </summary>
	/// <param name="content">Text to spoiler.</param>
	/// <returns>Formatted text.</returns>
	public static string Spoiler(this string content)
		=> $"||{content}||";

	/// <summary>
	/// Creates underlined text.
	/// </summary>
	/// <param name="content">Text to underline.</param>
	/// <returns>Formatted text.</returns>
	public static string Underline(this string content)
		=> $"__{content}__";

	/// <summary>
	/// Creates strikethrough text.
	/// </summary>
	/// <param name="content">Text to strikethrough.</param>
	/// <returns>Formatted text.</returns>
	public static string Strike(this string content)
		=> $"~~{content}~~";

	/// <summary>
	/// Creates a URL that won't create a link preview.
	/// </summary>
	/// <param name="url">Url to prevent from being previewed.</param>
	/// <returns>Formatted url.</returns>
	public static string EmbedlessUrl(this Uri url)
		=> $"<{url}>";

	/// <summary>
	/// Creates a masked link. This link will display as specified text, and alternatively provided alt text. This can only be used in embeds.
	/// </summary>
	/// <param name="text">Text to display the link as.</param>
	/// <param name="url">Url that the link will lead to.</param>
	/// <param name="altText">Alt text to display on hover.</param>
	/// <returns>Formatted url.</returns>
	public static string MaskedUrl(this string text, Uri url, string altText = "")
		=> $"[{text}]({url}{(!string.IsNullOrWhiteSpace(altText) ? $" \"{altText}\"" : "")})";

	/// <summary>
	/// Escapes all markdown formatting from specified text.
	/// </summary>
	/// <param name="text">Text to sanitize.</param>
	/// <returns>Sanitized text.</returns>
	public static string Sanitize(this string text)
		=> s_mdSanitizeRegex.Replace(text, m => $"\\{m.Groups[1].Value}");

	/// <summary>
	/// Removes all markdown formatting from specified text.
	/// </summary>
	/// <param name="text">Text to strip of formatting.</param>
	/// <returns>Formatting-stripped text.</returns>
	public static string Strip(this string text)
		=> s_mdStripRegex.Replace(text, m => string.Empty);

	/// <summary>
	/// Creates a mention for specified user or member. Can optionally specify to resolve nicknames.
	/// </summary>
	/// <param name="user">User to create mention for.</param>
	/// <param name="nickname">Whether the mention should resolve nicknames or not.</param>
	/// <returns>Formatted mention.</returns>
	public static string Mention(this DiscordUser user, bool nickname = false)
		=> nickname
		? $"<@!{user.Id.ToString(CultureInfo.InvariantCulture)}>"
		: $"<@{user.Id.ToString(CultureInfo.InvariantCulture)}>";

	/// <summary>
	/// Creates a mention for specified channel.
	/// </summary>
	/// <param name="channel">Channel to mention.</param>
	/// <returns>Formatted mention.</returns>
	public static string Mention(this DiscordChannel channel)
		=> $"<#{channel.Id.ToString(CultureInfo.InvariantCulture)}>";

	/// <summary>
	/// Creates a mention for specified role.
	/// </summary>
	/// <param name="role">Role to mention.</param>
	/// <returns>Formatted mention.</returns>
	public static string Mention(this DiscordRole role)
		=> $"<@&{role.Id.ToString(CultureInfo.InvariantCulture)}>";

	/// <summary>
	/// Creates a custom emoji string.
	/// </summary>
	/// <param name="emoji">Emoji to display.</param>
	/// <returns>Formatted emoji.</returns>
	public static string Emoji(this DiscordEmoji emoji)
		=> $"<:{emoji.Name}:{emoji.Id.ToString(CultureInfo.InvariantCulture)}>";

	/// <summary>
	/// Creates a url for using attachments in embeds. This can only be used as an Image URL, Thumbnail URL, Author icon URL or Footer icon URL.
	/// </summary>
	/// <param name="filename">Name of attached image to display</param>
	/// <returns></returns>
	public static string AttachedImageUrl(this string filename)
		=> $"attachment://{filename}";
}
