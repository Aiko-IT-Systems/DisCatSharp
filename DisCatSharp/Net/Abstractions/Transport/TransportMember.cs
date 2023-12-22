using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a transport member.
/// </summary>
internal class TransportMember : ObservableApiObject
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
	public TransportUser? User { get; internal set; }

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
	/// Gets the unusual dm activity time.
	/// </summary>
	[JsonProperty("unusual_dm_activity_until", NullValueHandling = NullValueHandling.Include)]
	public DateTime? UnusualDmActivityUntil { get; internal set; }

	/// <summary>
	/// Gets the members flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MemberFlags MemberFlags { get; internal set; }

	/// <summary>
	/// Gets the members permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? Permissions { get; internal set; }

	/// <summary>
	/// Gets ID of the guild to which this member belongs.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }
}
