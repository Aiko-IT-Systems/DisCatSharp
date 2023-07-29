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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Text;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
/// Internal tools.
/// </summary>
public static class Internals
{
	/// <summary>
	/// Gets the version of the library
	/// </summary>
	private static string s_versionHeader
		=> Utilities.VersionHeader;

	/// <summary>
	/// Gets the permission strings.
	/// </summary>
	private static Dictionary<Permissions, string> s_permissionStrings
		=> Utilities.PermissionStrings;

	/// <summary>
	/// Gets the utf8 encoding
	/// </summary>
	internal static UTF8Encoding Utf8
		=> Utilities.UTF8;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is joinable via voice.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsVoiceJoinable(this DiscordChannel channel)
		=> channel.Type
			is ChannelType.Voice
			or ChannelType.Stage;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> can have threads.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsThreadHolder(this DiscordChannel channel)
		=> channel.Type
			is ChannelType.Text
			or ChannelType.News
			or ChannelType.Forum;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is related to threads.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsThread(this DiscordChannel channel)
		=> channel.Type
			is ChannelType.PublicThread
			or ChannelType.PrivateThread
			or ChannelType.NewsThread;

	/// <summary>
	/// Whether users can write the <see cref="DiscordChannel"/>.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsWritable(this DiscordChannel channel)
		=> channel.Type
			is ChannelType.PublicThread
			or ChannelType.PrivateThread
			or ChannelType.NewsThread
			or ChannelType.Text
			or ChannelType.News
			or ChannelType.Group
			or ChannelType.Private
			or ChannelType.Voice;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is moveable in a parent.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsMovableInParent(this DiscordChannel channel)
		=> channel.Type
			is ChannelType.Voice
			or ChannelType.Stage
			or ChannelType.Text
			or ChannelType.Forum
			or ChannelType.News;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is moveable.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsMovable(this DiscordChannel channel)
		=> channel.Type == ChannelType.Category || channel.IsMovableInParent();
}
