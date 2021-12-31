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
using System.Text.RegularExpressions;
using DisCatSharp.Entities;

namespace DisCatSharp
{
    /// <summary>
    /// Contains markdown formatting helpers.
    /// </summary>
    public static class Formatter
    {
        /// <summary>
        /// Gets the md sanitize regex.
        /// </summary>
        private static Regex MdSanitizeRegex { get; } = new Regex(@"([`\*_~<>\[\]\(\)""@\!\&#:\|])", RegexOptions.ECMAScript);
        /// <summary>
        /// Gets the md strip regex.
        /// </summary>
        private static Regex MdStripRegex { get; } = new Regex(@"([`\*_~\[\]\(\)""\|]|<@\!?\d+>|<#\d+>|<@\&\d+>|<:[a-zA-Z0-9_\-]:\d+>)", RegexOptions.ECMAScript);

        /// <summary>
        /// Creates a block of code.
        /// </summary>
        /// <param name="Content">Contents of the block.</param>
        /// <param name="Language">Language to use for highlighting.</param>
        /// <returns>Formatted block of code.</returns>
        public static string BlockCode(string Content, string Language = "")
            => $"```{Language}\n{Content}\n```";

        /// <summary>
        /// Creates inline code snippet.
        /// </summary>
        /// <param name="Content">Contents of the snippet.</param>
        /// <returns>Formatted inline code snippet.</returns>
        public static string InlineCode(string Content)
            => $"`{Content}`";

        /// <summary>
        /// Creates a rendered timestamp.
        /// </summary>
        /// <param name="Time">The time from now.</param>
        /// <param name="Format">The format to render the timestamp in. Defaults to relative.</param>
        /// <returns>A formatted timestamp.</returns>
        public static string Timestamp(TimeSpan Time, TimestampFormat Format = TimestampFormat.RelativeTime)
            => Timestamp(DateTimeOffset.UtcNow + Time, Format);

        /// <summary>
        /// Creates a rendered timestamp.
        /// </summary>
        /// <param name="Time">Timestamp to format.</param>
        /// <param name="Format">The format to render the timestamp in. Defaults to relative.</param>
        /// <returns>A formatted timestamp.</returns>
        public static string Timestamp(DateTimeOffset Time, TimestampFormat Format = TimestampFormat.RelativeTime)
            => $"<t:{Time.ToUnixTimeSeconds()}:{(char)Format}>";

        /// <summary>
        /// Creates a rendered timestamp.
        /// </summary>
        /// <param name="Time">The time from now.</param>
        /// <param name="Format">The format to render the timestamp in. Defaults to relative.</param>
        /// <returns>A formatted timestamp relative to now.</returns>
        public static string Timestamp(DateTime Time, TimestampFormat Format = TimestampFormat.RelativeTime)
            => Timestamp(Time.ToUniversalTime() - DateTime.UtcNow, Format);

        /// <summary>
        /// Creates bold text.
        /// </summary>
        /// <param name="Content">Text to bolden.</param>
        /// <returns>Formatted text.</returns>
        public static string Bold(string Content)
            => $"**{Content}**";

        /// <summary>
        /// Creates italicized text.
        /// </summary>
        /// <param name="Content">Text to italicize.</param>
        /// <returns>Formatted text.</returns>
        public static string Italic(string Content)
            => $"*{Content}*";

        /// <summary>
        /// Creates spoiler from text.
        /// </summary>
        /// <param name="Content">Text to spoilerize.</param>
        /// <returns>Formatted text.</returns>
        public static string Spoiler(string Content)
            => $"||{Content}||";

        /// <summary>
        /// Creates underlined text.
        /// </summary>
        /// <param name="Content">Text to underline.</param>
        /// <returns>Formatted text.</returns>
        public static string Underline(string Content)
            => $"__{Content}__";

        /// <summary>
        /// Creates strikethrough text.
        /// </summary>
        /// <param name="Content">Text to strikethrough.</param>
        /// <returns>Formatted text.</returns>
        public static string Strike(string Content)
            => $"~~{Content}~~";

        /// <summary>
        /// Creates a URL that won't create a link preview.
        /// </summary>
        /// <param name="Url">Url to prevent from being previewed.</param>
        /// <returns>Formatted url.</returns>
        public static string EmbedlessUrl(Uri Url)
            => $"<{Url}>";

        /// <summary>
        /// Creates a masked link. This link will display as specified text, and alternatively provided alt text. This can only be used in embeds.
        /// </summary>
        /// <param name="Text">Text to display the link as.</param>
        /// <param name="Url">Url that the link will lead to.</param>
        /// <param name="alt_text">Alt text to display on hover.</param>
        /// <returns>Formatted url.</returns>
        public static string MaskedUrl(string Text, Uri Url, string AltText = "")
            => $"[{Text}]({Url}{(!string.IsNullOrWhiteSpace(AltText) ? $" \"{AltText}\"" : "")})";

        /// <summary>
        /// Escapes all markdown formatting from specified text.
        /// </summary>
        /// <param name="Text">Text to sanitize.</param>
        /// <returns>Sanitized text.</returns>
        public static string Sanitize(string Text)
            => MdSanitizeRegex.Replace(Text, M => $"\\{M.Groups[1].Value}");

        /// <summary>
        /// Removes all markdown formatting from specified text.
        /// </summary>
        /// <param name="Text">Text to strip of formatting.</param>
        /// <returns>Formatting-stripped text.</returns>
        public static string Strip(string Text)
            => MdStripRegex.Replace(Text, M => string.Empty);

        /// <summary>
        /// Creates a mention for specified user or member. Can optionally specify to resolve nicknames.
        /// </summary>
        /// <param name="User">User to create mention for.</param>
        /// <param name="Nickname">Whether the mention should resolve nicknames or not.</param>
        /// <returns>Formatted mention.</returns>
        public static string Mention(DiscordUser User, bool Nickname = false)
            => Nickname
            ? $"<@!{User.Id.ToString(CultureInfo.InvariantCulture)}>"
            : $"<@{User.Id.ToString(CultureInfo.InvariantCulture)}>";

        /// <summary>
        /// Creates a mention for specified channel.
        /// </summary>
        /// <param name="Channel">Channel to mention.</param>
        /// <returns>Formatted mention.</returns>
        public static string Mention(DiscordChannel Channel)
            => $"<#{Channel.Id.ToString(CultureInfo.InvariantCulture)}>";

        /// <summary>
        /// Creates a mention for specified role.
        /// </summary>
        /// <param name="Role">Role to mention.</param>
        /// <returns>Formatted mention.</returns>
        public static string Mention(DiscordRole Role)
            => $"<@&{Role.Id.ToString(CultureInfo.InvariantCulture)}>";

        /// <summary>
        /// Creates a custom emoji string.
        /// </summary>
        /// <param name="Emoji">Emoji to display.</param>
        /// <returns>Formatted emoji.</returns>
        public static string Emoji(DiscordEmoji Emoji)
            => $"<:{Emoji.Name}:{Emoji.Id.ToString(CultureInfo.InvariantCulture)}>";

        /// <summary>
        /// Creates a url for using attachments in embeds. This can only be used as an Image URL, Thumbnail URL, Author icon URL or Footer icon URL.
        /// </summary>
        /// <param name="Filename">Name of attached image to display</param>
        /// <returns></returns>
        public static string AttachedImageUrl(string Filename)
            => $"attachment://{Filename}";
    }
}
