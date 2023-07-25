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

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an onboarding prompt option.
/// </summary>
public sealed class DiscordOnboardingPromptOption : SnowflakeObject
{
	/// <summary>
	/// Gets the option's title.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the option's description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	/// Gets the option's emoji.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmoji? Emoji { get; internal set; }

	/// <summary>
	/// Gets the option's role ids.
	/// </summary>
	[JsonProperty("role_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong>? RoleIds { get; internal set; }
	
	/// <summary>
	/// Gets the option's channel ids.
	/// </summary>
	[JsonProperty("channel_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong>? ChannelIds { get; internal set; }

	/// <summary>
	/// Constructs a new onboarding prompt option.
	/// <para>You need to either specify <paramref name="roleIds"/> and/or <paramref name="channelIds"/>.</para>
	/// </summary>
	/// <param name="title">The title.</param>
	/// <param name="description">The description. Defaults to <see langword="null"/>.</param>
	/// <param name="emoji">The emoji. Defaults to <see langword="null"/>.</param>
	/// <param name="roleIds">The role ids. Defaults to <see langword="null"/>.</param>
	/// <param name="channelIds">The channel ids. Defaults to <see langword="null"/>.</param>
	public DiscordOnboardingPromptOption(string title, string? description = null,
		DiscordEmoji? emoji = null, List<ulong>? roleIds = null, List<ulong>? channelIds = null)
	{
		this.Title = title;
		this.Description = description;
		this.Emoji = emoji;
		this.RoleIds = roleIds;
		this.ChannelIds = channelIds;
	}

	/// <summary>
	/// Constructs a new onboarding prompt option.
	/// </summary>
	internal DiscordOnboardingPromptOption()
	{ }
}
