using System;

using DisCatSharp.Exceptions;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.RateLimitHit"/> event.
/// </summary>
public class RateLimitExceptionEventArgs : DiscordEventArgs
{
	public RateLimitException Exception { get; internal set; }

	public string ApiEndpoint { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="HeartbeatEventArgs"/> class.
	/// </summary>
	internal RateLimitExceptionEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
