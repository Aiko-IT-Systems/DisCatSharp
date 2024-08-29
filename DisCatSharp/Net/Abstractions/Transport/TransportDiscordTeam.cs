using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     The transport team.
/// </summary>
public sealed class TransportDiscordTeam : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="TransportDiscordTeam" /> class.
	/// </summary>
	internal TransportDiscordTeam()
	{ }

	/// <summary>
	///     Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
	public string Name { get; set; }

	/// <summary>
	///     Gets or sets the icon hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
	public string IconHash { get; set; }

	/// <summary>
	///     Gets or sets the owner id.
	/// </summary>
	[JsonProperty("owner_user_id")]
	public ulong OwnerId { get; set; }

	/// <summary>
	///     Gets or sets the members.
	/// </summary>
	[JsonProperty("members", NullValueHandling = NullValueHandling.Include)]
	public List<TransportDiscordTeamMember> Members { get; set; }
}
