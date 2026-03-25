// ReSharper disable InconsistentNaming

namespace DisCatSharp.Enums;

/// <summary>
///     The oauth scopes.
/// </summary>
public static class OAuth
{
	/// <summary>
	///     The default scopes for bots.
	/// </summary>
	private const string BOT_DEFAULT = "bot applications.commands"; // applications.commands.permissions.update

	/// <summary>
	///     The bot minimal scopes.
	/// </summary>
	private const string BOT_MINIMAL = "bot applications.commands";

	/// <summary>
	///     The bot only scope.
	/// </summary>
	private const string BOT_ONLY = "bot";

	/// <summary>
	///     The basic identify scopes.
	/// </summary>
	private const string IDENTIFY_BASIC = "identify email";

	/// <summary>
	///     The extended identify scopes.
	/// </summary>
	private const string IDENTIFY_EXTENDED = "identify email guilds guilds.members.read connections";

	/// <summary>
	///     The role connection scope.
	/// </summary>
	private const string ROLE_CONNECTIONS_WRITE = "role_connections.write";

	/// <summary>
	///     The application identities write scope.
	/// </summary>
	private const string APPLICATION_IDENTITIES_WRITE = "application_identities.write";

	/// <summary>
	///     The social layer SDK presence scope.
	/// </summary>
	private const string SDK_SOCIAL_LAYER_PRESENCE = "sdk.social_layer_presence";

	/// <summary>
	///     The openid scope.
	/// </summary>
	private const string OPENID = "openid";

	/// <summary>
	///     The social layer SDK communications scope.
	/// </summary>
	private const string SDK_SOCIAL_LAYER = "sdk.social_layer";

	/// <summary>
	///     The social layer SDK default presence scopes.
	/// </summary>
	private const string SDK_DEFAULT_PRESENCE = OPENID + " " + SDK_SOCIAL_LAYER_PRESENCE;

	/// <summary>
	///     The social layer SDK default communication scopes.
	/// </summary>
	private const string SDK_DEFAULT_COMMUNICATION = OPENID + " " + SDK_SOCIAL_LAYER;

	/// <summary>
	///     All scopes for bots and identify.
	/// </summary>
	private const string ALL = BOT_DEFAULT + " " + IDENTIFY_EXTENDED + " " + ROLE_CONNECTIONS_WRITE;

	/// <summary>
	///     Resolves the scopes.
	/// </summary>
	/// <param name="scope">The scope.</param>
	/// <returns>A string representing the scopes.</returns>
	public static string ResolveScopes(OAuthScopes scope) =>
		scope switch
		{
			OAuthScopes.BOT_DEFAULT => BOT_DEFAULT,
			OAuthScopes.BOT_MINIMAL => BOT_MINIMAL,
			OAuthScopes.BOT_ONLY => BOT_ONLY,
			OAuthScopes.IDENTIFY_BASIC => IDENTIFY_BASIC,
			OAuthScopes.IDENTIFY_EXTENDED => IDENTIFY_EXTENDED,
			OAuthScopes.ALL => ALL,
			OAuthScopes.ROLE_CONNECTIONS_WRITE => ROLE_CONNECTIONS_WRITE,
			OAuthScopes.APPLICATION_IDENTITIES_WRITE => APPLICATION_IDENTITIES_WRITE,
			OAuthScopes.SDK_SOCIAL_LAYER_PRESENCE => SDK_SOCIAL_LAYER_PRESENCE,
			OAuthScopes.OPENID => OPENID,
			OAuthScopes.SDK_SOCIAL_LAYER => SDK_SOCIAL_LAYER,
			OAuthScopes.SDK_DEFAULT_PRESENCE => SDK_DEFAULT_PRESENCE,
			OAuthScopes.SDK_DEFAULT_COMMUNICATION => SDK_DEFAULT_COMMUNICATION,
			_ => BOT_DEFAULT
		};
}

/// <summary>
///     The oauth scopes.
/// </summary>
public enum OAuthScopes
{
	/// <summary>
	///     Scopes: bot applications.commands (Excluding applications.commands.permissions.update for now)
	/// </summary>
	BOT_DEFAULT = 0,

	/// <summary>
	///     Scopes: bot applications.commands
	/// </summary>
	BOT_MINIMAL = 1,

	/// <summary>
	///     Scopes: bot
	/// </summary>
	BOT_ONLY = 2,

	/// <summary>
	///     Scopes: identify email
	/// </summary>
	IDENTIFY_BASIC = 3,

	/// <summary>
	///     Scopes: identify email guilds connections
	/// </summary>
	IDENTIFY_EXTENDED = 4,

	/// <summary>
	///     Scopes: bot applications.commands applications.commands.permissions.update identify email guilds connections
	///     role_connections.write
	/// </summary>
	ALL = 5,

	/// <summary>
	///     Scopes: role_connections.write
	/// </summary>
	ROLE_CONNECTIONS_WRITE = 6,

	/// <summary>
	///     Scopes: application_identities.write
	/// </summary>
	APPLICATION_IDENTITIES_WRITE = 7,

	/// <summary>
	///     Scopes: sdk.social_layer_presence
	/// </summary>
	SDK_SOCIAL_LAYER_PRESENCE = 8,

	/// <summary>
	///     Scopes: openid
	/// </summary>
	OPENID = 9,

	/// <summary>
	///     Scopes: sdk.social_layer
	/// </summary>
	SDK_SOCIAL_LAYER = 10,

	/// <summary>
	///     Scopes: openid sdk.social_layer_presence
	/// </summary>
	SDK_DEFAULT_PRESENCE = 11,

	/// <summary>
	///     Scopes: openid sdk.social_layer
	/// </summary>
	SDK_DEFAULT_COMMUNICATION = 12
}
