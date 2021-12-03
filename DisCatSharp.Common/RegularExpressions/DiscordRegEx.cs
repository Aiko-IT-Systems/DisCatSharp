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
using System.Text;
using System.Text.RegularExpressions;

namespace DisCatSharp.Common.RegularExpressions
{
    /// <summary>
    /// Provides common regex for discord related things.
    /// </summary>
    public static class DiscordRegEx
    {
        /// <summary>
        /// Represents a invite.
        /// </summary>
        public static Regex Invite
            => new(@"^(https?:\/\/)?(www\.)?(discord\.(gg|io|me|li)|discordapp\.com\/invite)\/(.+[a-z])$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        /// <summary>
        /// Represents a message link.
        /// </summary>
        public static Regex MessageLink
            => new(@"^\/channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        /// <summary>
        /// Represents a emoji.
        /// </summary>
        public static Regex Emoji
            => new(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        /// <summary>
        /// Represents a hex color string.
        /// </summary>
        public static Regex HexColorString
            => new(@"^#?([a-fA-F0-9]{6})$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        /// <summary>
        /// Represents a rgp color string.
        /// </summary>
        public static Regex RgbColorString
            => new(@"^(\d{1,3})\s*?,\s*?(\d{1,3}),\s*?(\d{1,3})$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        /// <summary>
        /// Represents a role.
        /// </summary>
        public static Regex Role
            => new(@"^<@&(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        /// <summary>
        /// Represents a channel.
        /// </summary>
        public static Regex Channel
            => new(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        /// <summary>
        /// Represents a user.
        /// </summary>
        public static Regex User
            => new(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
    }
}
