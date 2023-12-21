using System.Globalization;
using DisCatSharp.EventArgs;
using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// The lavalink voice server update.
/// </summary>
internal sealed class LavalinkVoiceServerUpdate
{
	/// <summary>
	/// Gets the token.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; }

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public string GuildId { get; }

	/// <summary>
	/// Gets the endpoint.
	/// </summary>
	[JsonProperty("endpoint")]
	public string Endpoint { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkVoiceServerUpdate"/> class.
	/// </summary>
	/// <param name="vsu">The vsu.</param>
	internal LavalinkVoiceServerUpdate(VoiceServerUpdateEventArgs vsu)
	{
		this.Token = vsu.VoiceToken;
		this.GuildId = vsu.Guild.Id.ToString(CultureInfo.InvariantCulture);
		this.Endpoint = vsu.Endpoint;
	}
}

/// <summary>
/// The lavalink voice update.
/// </summary>
internal sealed class LavalinkVoiceUpdate : LavalinkPayload
{
	/// <summary>
	/// Gets the session id.
	/// </summary>
	[JsonProperty("sessionId")]
	public string SessionId { get; }

	/// <summary>
	/// Gets the event.
	/// </summary>
	[JsonProperty("event")]
	internal LavalinkVoiceServerUpdate Event { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkVoiceUpdate"/> class.
	/// </summary>
	/// <param name="vstu">The vstu.</param>
	/// <param name="vsrvu">The vsrvu.</param>
	public LavalinkVoiceUpdate(VoiceStateUpdateEventArgs vstu, VoiceServerUpdateEventArgs vsrvu)
		: base("voiceUpdate", vstu.Guild.Id.ToString(CultureInfo.InvariantCulture))
	{
		this.SessionId = vstu.SessionId;
		this.Event = new LavalinkVoiceServerUpdate(vsrvu);
	}
}
