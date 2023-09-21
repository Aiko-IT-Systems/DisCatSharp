using Microsoft.Extensions.Logging;

namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Contains well-defined event IDs used by the Lavalink extension.
/// </summary>
public static class LavalinkEvents
{
	/// <summary>
	/// Miscellaneous events, that do not fit in any other category.
	/// </summary>
	public static EventId Misc { get; } = new(400, "Lavalink");

	/// <summary>
	/// Events pertaining to Lavalink node connection errors.
	/// </summary>
	public static EventId LavalinkConnectionError { get; } = new(401, nameof(LavalinkConnectionError));

	/// <summary>
	/// Events emitted for clean disconnects from Lavalink.
	/// </summary>
	public static EventId LavalinkSessionConnectionClosed { get; } = new(402, nameof(LavalinkSessionConnectionClosed));

	/// <summary>
	/// Events emitted for successful connections made to Lavalink.
	/// </summary>
	public static EventId LavalinkSessionConnected { get; } = new(403, nameof(LavalinkSessionConnected));

	/// <summary>
	/// Events emitted when the Lavalink REST API responds with an error.
	/// </summary>
	public static EventId LavalinkRestError { get; } = new(404, nameof(LavalinkRestError));

	/// <summary>
	/// Events containing raw payloads, received from Lavalink nodes.
	/// </summary>
	public static EventId LavalinkWsRx { get; } = new(405, "Lavalink ↓");

	/// <summary>
	/// Events emitted when the Lavalink WebSocket connection sends invalid data.
	/// </summary>
	public static EventId LavalinkWsException { get; } = new(407, nameof(LavalinkWsException));

	/// <summary>
	/// Events pertaining to Gateway Intents. Typically diagnostic information.
	/// </summary>
	public static EventId Intents { get; } = new(499, nameof(Intents));
}
