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

using System.Collections.Generic;
using System.Text;
using DisCatSharp.Entities;

namespace DisCatSharp
{
    /// <summary>
    /// Internal tools.
    /// </summary>
    public static class Internals
    {
        /// <summary>
        /// Gets the version of the library
        /// </summary>
        private static string VersionHeader
            => Utilities.VersionHeader;

        /// <summary>
        /// Gets the permission strings.
        /// </summary>
        private static Dictionary<Permissions, string> PermissionStrings
            => Utilities.PermissionStrings;

        /// <summary>
        /// Gets the utf8 encoding
        /// </summary>
        internal static UTF8Encoding Utf8
            => Utilities.Utf8;

        /// <summary>
        /// Initializes a new instance of the <see cref="Internals"/> class.
        /// </summary>
        static Internals() { }

        /// <summary>
        /// Whether the <see cref="DiscordChannel"/> is joinable via voice.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        internal static bool IsVoiceJoinable(this DiscordChannel Channel) => Channel.Type == ChannelType.Voice || Channel.Type == ChannelType.Stage;

        /// <summary>
        /// Whether the <see cref="DiscordChannel"/> can have threads.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        internal static bool IsThreadHolder(this DiscordChannel Channel) => Channel.Type == ChannelType.Text || Channel.Type == ChannelType.News || Channel.Type == ChannelType.GuildForum;

        /// <summary>
        /// Whether the <see cref="DiscordChannel"/> is related to threads.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        internal static bool IsThread(this DiscordChannel Channel) => Channel.Type == ChannelType.PublicThread || Channel.Type == ChannelType.PrivateThread || Channel.Type == ChannelType.NewsThread;

        /// <summary>
        /// Whether users can write the <see cref="DiscordChannel"/>.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        internal static bool IsWriteable(this DiscordChannel Channel) => Channel.Type == ChannelType.PublicThread || Channel.Type == ChannelType.PrivateThread || Channel.Type == ChannelType.NewsThread || Channel.Type == ChannelType.Text || Channel.Type == ChannelType.News || Channel.Type == ChannelType.Group || Channel.Type == ChannelType.Private || Channel.Type == ChannelType.Voice;

        /// <summary>
        /// Whether the <see cref="DiscordChannel"/> is moveable in a parent.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        internal static bool IsMovableInParent(this DiscordChannel Channel) => Channel.Type == ChannelType.Voice || Channel.Type == ChannelType.Stage || Channel.Type == ChannelType.Text || Channel.Type == ChannelType.GuildForum || Channel.Type == ChannelType.News || Channel.Type == ChannelType.Store;

        /// <summary>
        /// Whether the <see cref="DiscordChannel"/> is moveable.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        internal static bool IsMovable(this DiscordChannel Channel) => Channel.Type == ChannelType.Voice || Channel.Type == ChannelType.Stage || Channel.Type == ChannelType.Text || Channel.Type == ChannelType.Category || Channel.Type == ChannelType.GuildForum || Channel.Type == ChannelType.News || Channel.Type == ChannelType.Store;
    }
}
