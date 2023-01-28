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

using System.Text.RegularExpressions;

namespace DisCatSharp.Common.RegularExpressions;

/// <summary>
/// Provides common regex for discord related things.
/// </summary>
public static class DiscordRegEx
{
	// language=regex
	private const string WEBSITE = @"(https?:\/\/)?(www\.|canary\.|ptb\.)?(discord|discordapp)\.com\/";

	/// <summary>
	/// Represents a invite.
	/// </summary>
	public static readonly Regex Invite
		= new($@"^((https?:\/\/)?(www\.)?discord\.gg(\/.*)*|{WEBSITE}invite)\/(?<code>[a-zA-Z0-9]*)(\?event=(?<event>\d+))?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a message link.
	/// </summary>
	public static readonly Regex MessageLink
		= new($@"^{WEBSITE}channels\/(?<guild>(?:\d+|@me))\/(?<channel>\d+)\/(?<message>\d+)\/?", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a emoji.
	/// </summary>
	public static readonly Regex Emoji
		= new(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a animated emoji.
	/// </summary>
	public static readonly Regex AnimatedEmoji
		= new(@"^<(?<animated>a):(?<name>\w{2,32}):(?<id>\d{17,20})>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a non-animated emoji.
	/// </summary>
	public static readonly Regex StaticEmoji
		= new(@"^<:(?<name>\w{2,32}):(?<id>\d{17,20})>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a timestamp.
	/// </summary>
	public static readonly Regex Timestamp
		= new(@"^<t:(?<timestamp>-?\d{1,13})(:(?<style>[tTdDfFR]))?>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a default styled timestamp.
	/// </summary>
	public static readonly Regex DefaultStyledTimestamp
		= new(@"^<t:(?<timestamp>-?\d{1,13})$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a styled timestamp.
	/// </summary>
	public static readonly Regex StyledTimestamp
		= new(@"^<t:(?<timestamp>-?\d{1,13}):(?<style>[tTdDfFR])>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a role.
	/// </summary>
	public static readonly Regex Role
		= new(@"^<@&(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a channel.
	/// </summary>
	public static readonly Regex Channel
		= new(@"^<#(\d+)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a user.
	/// </summary>
	public static readonly Regex User
		= new(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a user with nickname.
	/// </summary>
	public static readonly Regex UserWithNickname
		= new(@"^<@\!(?<id>\d{17,20})>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a user with optional nickname.
	/// </summary>
	public static readonly Regex UserWithOptionalNickname
		= new(@"^<@\!?(?<id>\d{17,20})>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a user without nickname.
	/// </summary>
	public static readonly Regex UserWithoutNickname
		= new(@"^<@(?<id>\d{17,20})>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

	/// <summary>
	/// Represents a event.
	/// </summary>
	public static readonly Regex Event
		= new($@"^{WEBSITE}events\/(?<guild>\d+)\/(?<event>\d+)$", RegexOptions.ECMAScript | RegexOptions.Compiled);
}
