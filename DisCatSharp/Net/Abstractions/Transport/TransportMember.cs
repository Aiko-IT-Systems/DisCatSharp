// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a transport member.
/// </summary>
internal class TransportMember
{
	/// <summary>
	/// Gets the avatar hash.
	/// </summary>
	[JsonIgnore]
	public string AvatarHash { get; internal set; }

	/// <summary>
	/// Gets the guild avatar hash.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	public string GuildAvatarHash { get; internal set; }

	/// <summary>
	/// Gets the guild banner hash.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public string GuildBannerHash { get; internal set; }

	/// <summary>
	/// Gets the guild bio.
	/// This is not available to bots tho.
	/// </summary>
	[JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
	public string GuildBio { get; internal set; }

	/// <summary>
	/// Gets the members's pronouns.
	/// </summary>
	[JsonProperty("pronouns", NullValueHandling = NullValueHandling.Ignore)]
	public string GuildPronouns { get; internal set; }

	/// <summary>
	/// Gets the user.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportUser User { get; internal set; }

	/// <summary>
	/// Gets the nickname.
	/// </summary>
	[JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
	public string Nickname { get; internal set; }

	/// <summary>
	/// Gets the roles.
	/// </summary>
	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> Roles { get; internal set; }

	/// <summary>
	/// Gets the joined at.
	/// </summary>
	[JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTime JoinedAt { get; internal set; }

	/// <summary>
	/// Whether this member is deafened.
	/// </summary>
	[JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsDeafened { get; internal set; }

	/// <summary>
	/// Whether this member is muted.
	/// </summary>
	[JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsMuted { get; internal set; }

	/// <summary>
	/// Gets the premium since.
	/// </summary>
	[JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
	public DateTime? PremiumSince { get; internal set; }

	/// <summary>
	/// Whether this member is marked as pending.
	/// </summary>
	[JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsPending { get; internal set; }

	/// <summary>
	/// Gets the timeout time.
	/// </summary>
	[JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Include)]
	public DateTime? CommunicationDisabledUntil { get; internal set; }

	/// <summary>
	/// Gets the members flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MemberFlags MemberFlags { get; internal set; }
}
