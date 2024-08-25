using DisCatSharp.Entities;
using DisCatSharp.Experimental.Enums;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents supplemental information about a user's join source in a guild.
/// </summary>
public sealed class DiscordSupplementalGuildMember : ObservableApiObject
{
	/// <summary>
	/// Gets the associated guild member.
	/// </summary>
	[JsonIgnore]
	public DiscordMember Member { get; internal set; }

	/// <summary>
	/// Gets the associated transport member.
	/// </summary>
	[JsonProperty("member")]
	internal TransportMember TransportMember { get; set; }

	/// <summary>
	/// Gets how the user joined the guild.
	/// </summary>
	[JsonProperty("join_source_type")]
	public JoinSourceType JoinSourceType { get; internal set; }

	/// <summary>
	/// Gets the invite code or vanity used to join the guild, if applicable.
	/// </summary>
	[JsonProperty("source_invite_code", NullValueHandling = NullValueHandling.Ignore)]
	public string? SourceInviteCode { get; internal set; }

	/// <summary>
	/// Gets the ID of the user who invited the user to the guild, if applicable.
	/// </summary>
	[JsonProperty("inviter_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? InviterId { get; internal set; }

	/// <summary>
	/// Gets the type of integration that added the user to the guild, if applicable.
	/// I.e. integration type (twitch, youtube, discord, or guild_subscription).
	/// </summary>
	[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
	public int? IntegrationType { get; internal set; }
}
