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

using DisCatSharp.Attributes;
using DisCatSharp.Entities;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a channel's flags.
/// </summary>
public enum ChannelFlags : int
{
	/// <summary>
	/// Indicates that this channel is removed from the guilds home feed / from highlights.
	/// Applicable for <see cref="ChannelType"/> Text, Forum and News.
	/// </summary>
	RemovedFromHome = 1 << 0,
	RemovedFromHighlights = RemovedFromHome,

	/// <summary>
	/// Indicates that this thread is pinned to the top of its parent forum channel.
	/// Forum channel thread only.
	/// </summary>
	Pinned = 1 << 1,

	/// <summary>
	/// Indicates that this channel is removed from the active now within the guilds home feed.
	/// Applicable for <see cref="ChannelType"/> Text, News, Thread, Forum, Stage and Voice.
	/// </summary>
	RemovedFromActiveNow = 1 << 2,

	/// <summary>
	/// Indicates that the channel requires users to select at least one <see cref="ForumPostTag"/>.
	/// Only applicable for <see cref="ChannelType.Forum"/>.
	/// </summary>
	RequireTags = 1<<4,

	/// <summary>
	/// Indicated that this channel is spam.
	/// </summary>
	IsSpam = 1<<5,

	/// <summary>
	/// Indicated that this channel is a guild resource channel.
	/// </summary>
	[DiscordInExperiment]
	IsGuildResourceChannel = 1<<7,

	/// <summary>
	/// Indicated that clyde has access to this thread.
	/// </summary>
	[DiscordInExperiment]
	ClydeAi = 1<<8,

	/// <summary>
	/// Unknown what this flag does. Releates to clyde thread?
	/// </summary>
	[DiscordUnreleased("We do not have information about this.")]
	IsScheduledForDeletion = 1<<9,

	/// <summary>
	/// Indicates that this channel is a media channel.
	/// </summary>
	[DiscordUnreleased]
	IsMediaChannel = 1<<10,

	/// <summary>
	/// Indicates that ai powered summaries are disabled for this channel.
	/// </summary>
	[DiscordInExperiment]
	SummariesDisabled = 1<<11,
	
	/// <summary>
	/// Currently unknown.
	/// </summary>
	[DiscordInExperiment]
	ApplicationShelfConsent = 1<<12,
	
	/// <summary>
	/// Indicates that this channel is part of a role subscription template preview.
	/// </summary>
	[DiscordInExperiment]
	IsRoleSubscriptionTemplatePreviewChannel = 1<<13
}
