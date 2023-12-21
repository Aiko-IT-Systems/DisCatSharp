using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// The lavalink payload.
/// </summary>
internal abstract class LavalinkPayload
{
	/// <summary>
	/// Gets the operation.
	/// </summary>
	[JsonProperty("op")]
	public string Operation { get; }

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guildId", NullValueHandling = NullValueHandling.Ignore)]
	public string GuildId { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkPayload"/> class.
	/// </summary>
	/// <param name="opcode">The opcode.</param>
	internal LavalinkPayload(string opcode)
	{
		this.Operation = opcode;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkPayload"/> class.
	/// </summary>
	/// <param name="opcode">The opcode.</param>
	/// <param name="guildId">The guild id.</param>
	internal LavalinkPayload(string opcode, string guildId)
	{
		this.Operation = opcode;
		this.GuildId = guildId;
	}
}
