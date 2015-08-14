using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Request guild members.
/// </summary>
internal sealed class GatewayRequestGuildMembers
{
	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; }

	/// <summary>
	/// Gets the query.
	/// </summary>
	[JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
	public string Query { get; set; }

	/// <summary>
	/// Gets the limit.
	/// </summary>
	[JsonProperty("limit")]
	public int Limit { get; set; }

	/// <summary>
	/// Gets whether presences should be returned.
	/// </summary>
	[JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Presences { get; set; }

	/// <summary>
	/// Gets the user ids.
	/// </summary>
	[JsonProperty("user_ids", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<ulong> UserIds { get; set; }

	/// <summary>
	/// Gets the nonce.
	/// </summary>
	[JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
	public string Nonce { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GatewayRequestGuildMembers"/> class.
	/// </summary>
	/// <param name="guild">The guild.</param>
	public GatewayRequestGuildMembers(DiscordGuild guild)
	{
		this.GuildId = guild.Id;
	}
}
