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

using System.Collections.Concurrent;
using System.Collections.Generic;

using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a stage instance to which the user is invited.
/// </summary>
public class DiscordInviteStage : SnowflakeObject
{
	/// <summary>
	/// Gets the members speaking in the Stage.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordMember> Members { get; internal set; }

	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
	[JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordMember> MembersInternal = new();

	/// <summary>
	/// Gets the number of users in the Stage.
	/// </summary>
	[JsonProperty("participant_count", NullValueHandling = NullValueHandling.Ignore)]
	public int ParticipantCount { get; internal set; }

	/// <summary>
	/// Gets the number of users speaking in the Stage.
	/// </summary>
	[JsonProperty("speaker_count", NullValueHandling = NullValueHandling.Ignore)]
	public int SpeakerCount { get; internal set; }

	/// <summary>
	/// Gets the topic of the Stage instance.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string Topic { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordInviteStage"/> class.
	/// </summary>
	internal DiscordInviteStage()
	{
		this.Members = new ReadOnlyConcurrentDictionary<ulong, DiscordMember>(this.MembersInternal);
	}
}
