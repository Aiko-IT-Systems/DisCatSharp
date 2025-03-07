using System;

using DisCatSharp.Exceptions;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.RateLimitHit" /> event.
/// </summary>
public class RateLimitExceptionEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="HeartbeatEventArgs" /> class.
	/// </summary>
	internal RateLimitExceptionEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the ratelimit exception.
	/// </summary>
	public RateLimitException Exception { get; internal set; }

	/// <summary>
	///     Gets the called api endpoint.
	/// </summary>
	public string ApiEndpoint { get; internal set; }
}
