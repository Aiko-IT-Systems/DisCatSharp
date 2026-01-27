using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

[Flags]
public enum GatewayCapabilities
{
	/// <summary>
	///     No capabilities are set.
	/// </summary>
	None = 0,

	/// <summary>
	///     Removes the notes field from the Ready event.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	LAZY_USER_NOTES = 1 << 0,

	/// <summary>
	///     Prevents member/presence syncing and Presence Update events for implicit relationships.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	NO_AFFINE_USER_IDS = 1 << 1,

	/// <summary>
	///     Enables versioned read states; read_state becomes an object and can be cached when re-identifying.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	VERSIONED_READ_STATES = 1 << 2,

	/// <summary>
	///     Enables versioned user guild settings; user_guild_settings becomes an object and can be cached when re-identifying.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	VERSIONED_USER_GUILD_SETTINGS = 1 << 3,

	/// <summary>
	///     Dehydrates the Ready payload, moving all user objects to users and replacing them with ids; merges guild members into merged_members.
	/// </summary>
	DEDUPE_USER_OBJECTS = 1 << 4,

	/// <summary>
	///     Splits the Ready payload into Ready and Ready Supplemental for faster initial receipt.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	PRIORITIZED_READY_PAYLOAD = 1 << 5,

	/// <summary>
	///     Changes guild_experiments populations to an array of populations in the Ready event.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	MULTIPLE_GUILD_EXPERIMENT_POPULATIONS = 1 << 6,

	/// <summary>
	///     Includes read states tied to non-channel resources (e.g., scheduled events, notification center).
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	NON_CHANNEL_READ_STATES = 1 << 7,

	/// <summary>
	///     Enables auth token refresh, optionally providing a new auth token in the Ready event.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	AUTH_TOKEN_REFRESH = 1 << 8,

	/// <summary>
	///     Removes user_settings from Ready and User Settings Update events; uses user_settings_proto instead.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	USER_SETTINGS_PROTO = 1 << 9,

	/// <summary>
	///     Enables client state caching v2.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	CLIENT_STATE_V2 = 1 << 10,

	/// <summary>
	///     Enables passive guild updates, replacing certain events with Passive Update V1 for unsubscribed guilds.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	PASSIVE_GUILD_UPDATE = 1 << 11,

	/// <summary>
	///     Connects to all pre-existing calls upon connecting to the Gateway.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	AUTO_CALL_CONNECT = 1 << 12,

	/// <summary>
	///     Debounces message reactions, emitting Message Reaction Add Many instead of rapid single adds.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	DEBOUNCE_MESSAGE_REACTIONS = 1 << 13,

	/// <summary>
	///     Enables passive guild updates v2, sending Passive Update V2 for unsubscribed guilds.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	PASSIVE_GUILD_UPDATE_V2 = 1 << 14,

	/// <summary>
	///     Obfuscates channel identifiers.
	/// </summary>
	[DiscordInExperiment]
	OBFUSCATED_CHANNELS = 1 << 15,

	/// <summary>
	///     Adds lobbies to Ready and stops streaming Lobby Create events on connect.
	/// </summary>
	/// <remarks>
	/// 	Not for bot accounts.
	/// </remarks>
	AUTO_LOBBY_CONNECT = 1 << 16
}
