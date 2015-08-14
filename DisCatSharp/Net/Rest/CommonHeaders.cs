namespace DisCatSharp.Net;

/// <summary>
/// Represents common headers used by the Discord API.
/// </summary>
public static class CommonHeaders
{
	/// <summary>
	/// Gets or sets the ratelimit scope header.
	/// </summary>
	public const string RATELIMIT_SCOPE = "x-ratelimit-scope";

	/// <summary>
	/// Gets or sets the ratelimit limit header.
	/// </summary>
	public const string RATELIMIT_LIMIT = "x-ratelimit-limit";

	/// <summary>
	/// Gets or sets the ratelimit remaining header.
	/// </summary>
	public const string RATELIMIT_REMAINING = "x-ratelimit-remaining";

	/// <summary>
	/// Gets or sets the ratelimit reset header.
	/// </summary>
	public const string RATELIMIT_RESET = "x-ratelimit-reset";

	/// <summary>
	/// Gets or sets the ratelimit reset after header.
	/// </summary>
	public const string RATELIMIT_RESET_AFTER = "x-ratelimit-reset-after";

	/// <summary>
	/// Gets or sets the ratelimit bucket header.
	/// </summary>
	public const string RATELIMIT_BUCKET = "x-ratelimit-bucket";

	/// <summary>
	/// Gets or sets the ratelimit global header.
	/// </summary>
	public const string RATELIMIT_GLOBAL = "x-ratelimit-global";

	/// <summary>
	/// Gets or sets the retry after header.
	/// </summary>
	public const string RETRY_AFTER = "Retry-After";

	/// <summary>
	/// Gets or sets the discord locale header.
	/// </summary>
	public const string DISCORD_LOCALE = "X-Discord-Locale";

	/// <summary>
	/// Gets or sets the discord timezone header.
	/// </summary>
	public const string DISCORD_TIMEZONE = "X-Discord-Timezone";

	/// <summary>
	/// Gets or sets the super properties header.
	/// </summary>
	public const string SUPER_PROPERTIES = "X-Super-Properties";

	/// <summary>
	/// Gets or sets the user agent header.
	/// </summary>
	public const string USER_AGENT = "UserAgent";

	/// <summary>
	/// Gets or sets the authorization header.
	/// </summary>
	public const string AUTHORIZATION = "Authorization";

	/// <summary>
	/// Gets or sets the authorization bearer header prefix.
	/// </summary>
	public const string AUTHORIZATION_BEARER = "Bearer";

	/// <summary>
	/// Gets or sets the authorization basic header prefix.
	/// </summary>
	public const string AUTHORIZATION_BASIC = "Basic";

	/// <summary>
	/// Gets or sets the authorization bot header prefix.
	/// </summary>
	public const string AUTHORIZATION_BOT = "Bot";

	/// <summary>
	/// Gets or sets the connection header.
	/// </summary>
	public const string CONNECTION = "Connection";

	/// <summary>
	/// Gets or sets the connection type header.
	/// </summary>
	public const string CONNECTION_KEEP_ALIVE = "keep-alive";

	/// <summary>
	/// Gets or sets the keep alive header.
	/// </summary>
	public const string KEEP_ALIVE = "Keep-Alive";

	/// <summary>
	/// Gets or sets the audit log reason header.
	/// </summary>
	public const string AUDIT_LOG_REASON_HEADER = "X-Audit-Log-Reason";
}
