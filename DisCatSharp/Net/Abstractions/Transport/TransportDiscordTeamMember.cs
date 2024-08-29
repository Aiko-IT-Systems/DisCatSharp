using System.Collections.Generic;

using DisCatSharp.Attributes;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     The transport team member.
/// </summary>
public sealed class TransportDiscordTeamMember
{
	/// <summary>
	///     Initializes a new instance of the <see cref="TransportDiscordTeamMember" /> class.
	/// </summary>
	internal TransportDiscordTeamMember()
	{ }

	/// <summary>
	///     Gets or sets the membership state.
	/// </summary>
	[JsonProperty("membership_state")]
	public int MembershipState { get; set; }

	/// <summary>
	///     Gets or sets the permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Include), DiscordDeprecated]
	public List<string> Permissions { get; set; }

	/// <summary>
	///     Gets the member's role within the team.
	///     <para>Can be <c>owner</c>, <c>admin</c>, <c>developer</c> or <c>read-only</c>.</para>
	/// </summary>
	[JsonProperty("role")]
	public string Role { get; set; }

	/// <summary>
	///     Gets or sets the team id.
	/// </summary>
	[JsonProperty("team_id")]
	public ulong TeamId { get; set; }

	/// <summary>
	///     Gets or sets the user.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Include)]
	public TransportDiscordUser DiscordUser { get; set; }
}
