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

using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a attachment flag extensions.
/// </summary>
public static class AttachmentFlagExtensions
{
	/// <summary>
	/// Calculates whether these attachment flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasAttachmentFlag(this AttachmentFlags baseFlags, AttachmentFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
/// Represents additional features of a attachment.
/// </summary>
[Flags]
public enum AttachmentFlags
{
	/// <summary>
	/// This attachment has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	/// This attachment is a thumbnail.
	/// </summary>
	[DiscordUnreleased]
	IsThumbnail = 1 << 1,

	/// <summary>
	/// This attachment was edited with remix (https://support.discord.com/hc/en-us/articles/15145601963031).
	/// </summary>
	[DiscordInExperiment]
	IsRemix = 1 << 2,

	/// <summary>
	/// This attachment is a flag.
	/// </summary>
	[DiscordUnreleased]
	IsClip = 1 << 3
}
