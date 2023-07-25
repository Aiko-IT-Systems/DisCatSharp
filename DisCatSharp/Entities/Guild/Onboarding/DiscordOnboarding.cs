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

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild's onboarding configuration.
/// </summary>
public sealed class DiscordOnboarding : ObservableApiObject
{
	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null!;

	/// <summary>
	/// Gets the onboarding prompts
	/// </summary>
	[JsonProperty("prompts", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordOnboardingPrompt> Prompts { get; internal set; } = new();

	/// <summary>
	/// Gets the default channel ids.
	/// </summary>
	[JsonProperty("default_channel_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> DefaultChannelIds { get; internal set; } = new();

	/// <summary>
	/// Gets whether onboarding is enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Enabled { get; internal set; }

	/// <summary>
	/// Gets the onboarding mode.
	/// </summary>
	[JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
	public OnboardingMode Mode { get; internal set; }

	/// <summary>
	/// Gets whether the current configuration is below discords requirements.
	/// </summary>
	[JsonProperty("below_requirements", NullValueHandling = NullValueHandling.Ignore)]
	public bool BelowRequirements { get; internal set; }

	/// <summary>
	/// Constructs a new onboarding configuration.
	/// </summary>
	internal DiscordOnboarding()
		:base(new() { "responses", "onboarding_prompts_seen", "onboarding_responses_seen" })
	{ }
}
