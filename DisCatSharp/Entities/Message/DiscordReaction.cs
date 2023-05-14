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

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a reaction to a message.
/// </summary>
public class DiscordReaction : ObservableApiObject
{
	/// <summary>
	/// Gets the total number of users who reacted with this emoji.
	/// </summary>
	[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
	public int Count { get; internal set; }

	/// <summary>
	/// Gets the total number of users who burst reacted with this emoji.
	/// </summary>
	[JsonProperty("burst_count", NullValueHandling = NullValueHandling.Ignore)]
	public int BurstCount { get; internal set; }

	/// <summary>
	/// Gets whether the current user burst reacted with this emoji.
	/// </summary>
	[JsonProperty("burst_me", NullValueHandling = NullValueHandling.Ignore)]
	public bool BurstMe { get; internal set; }

	/// <summary>
	/// Gets the ids of users who burst reacted with this emoji.
	/// </summary>
	[JsonProperty("burst_user_ids", NullValueHandling = NullValueHandling.Ignore)]
	public ulong[] BurstUserIds { get; internal set; }

	/// <summary>
	/// Gets the burst colors.
	/// </summary>
	[JsonProperty("burst_colors", NullValueHandling = NullValueHandling.Ignore)]
	public string[] BurstColors { get; internal set; }

	/// <summary>
	/// Gets whether the current user reacted with this emoji.
	/// </summary>
	[JsonProperty("me", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsMe { get; internal set; }

	/// <summary>
	/// Gets the emoji used to react to this message.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmoji Emoji { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordReaction"/> class.
	/// </summary>
	internal DiscordReaction()
	{ }
}
