namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Contains documented Discord webhook event type names.
/// </summary>
public static class DiscordWebhookEventNames
{
	/// <summary>
	///     Sent when an app was authorized by a user.
	/// </summary>
	public const string ApplicationAuthorized = "APPLICATION_AUTHORIZED";

	/// <summary>
	///     Sent when an app was deauthorized by a user.
	/// </summary>
	public const string ApplicationDeauthorized = "APPLICATION_DEAUTHORIZED";

	/// <summary>
	///     Sent when an entitlement was created.
	/// </summary>
	public const string EntitlementCreate = "ENTITLEMENT_CREATE";

	/// <summary>
	///     Sent when an entitlement was updated.
	/// </summary>
	public const string EntitlementUpdate = "ENTITLEMENT_UPDATE";

	/// <summary>
	///     Sent when an entitlement was deleted.
	/// </summary>
	public const string EntitlementDelete = "ENTITLEMENT_DELETE";

	/// <summary>
	///     Sent when a user is enrolled into a quest.
	/// </summary>
	public const string QuestUserEnrollment = "QUEST_USER_ENROLLMENT";

	/// <summary>
	///     Sent when a lobby message is created.
	/// </summary>
	public const string LobbyMessageCreate = "LOBBY_MESSAGE_CREATE";

	/// <summary>
	///     Sent when a lobby message is updated.
	/// </summary>
	public const string LobbyMessageUpdate = "LOBBY_MESSAGE_UPDATE";

	/// <summary>
	///     Sent when a lobby message is deleted.
	/// </summary>
	public const string LobbyMessageDelete = "LOBBY_MESSAGE_DELETE";

	/// <summary>
	///     Sent when a Social SDK game direct message is created.
	/// </summary>
	public const string GameDirectMessageCreate = "GAME_DIRECT_MESSAGE_CREATE";

	/// <summary>
	///     Sent when a Social SDK game direct message is updated.
	/// </summary>
	public const string GameDirectMessageUpdate = "GAME_DIRECT_MESSAGE_UPDATE";

	/// <summary>
	///     Sent when a Social SDK game direct message is deleted.
	/// </summary>
	public const string GameDirectMessageDelete = "GAME_DIRECT_MESSAGE_DELETE";
}
