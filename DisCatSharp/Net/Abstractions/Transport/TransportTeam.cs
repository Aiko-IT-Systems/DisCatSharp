using System.Collections.Generic;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// The transport team.
/// </summary>
internal sealed class TransportTeam : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the icon hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
	public string IconHash { get; set; }

	/// <summary>
	/// Gets or sets the owner id.
	/// </summary>
	[JsonProperty("owner_user_id")]
	public ulong OwnerId { get; set; }

	/// <summary>
	/// Gets or sets the members.
	/// </summary>
	[JsonProperty("members", NullValueHandling = NullValueHandling.Include)]
	public List<TransportTeamMember> Members { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TransportTeam"/> class.
	/// </summary>
	internal TransportTeam()
	{ }
}

/// <summary>
/// The transport team member.
/// </summary>
internal sealed class TransportTeamMember
{
	/// <summary>
	/// Gets or sets the membership state.
	/// </summary>
	[JsonProperty("membership_state")]
	public int MembershipState { get; set; }

	/// <summary>
	/// Gets or sets the permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Include), DiscordDeprecated]
	public List<string> Permissions { get; set; }

	/// <summary>
	/// Gets the member's role within the team.
	/// <para>Can be <c>owner</c>, <c>admin</c>, <c>developer</c> or <c>read-only</c>.</para>
	/// </summary>
	[JsonProperty("role")]
	public string Role { get; set; }

	/// <summary>
	/// Gets or sets the team id.
	/// </summary>
	[JsonProperty("team_id")]
	public ulong TeamId { get; set; }

	/// <summary>
	/// Gets or sets the user.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Include)]
	public TransportUser User { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TransportTeamMember"/> class.
	/// </summary>
	internal TransportTeamMember()
	{ }
}
