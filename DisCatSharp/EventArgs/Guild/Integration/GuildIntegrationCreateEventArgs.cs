using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildIntegrationCreated"/> event.
/// </summary>
public class GuildIntegrationCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the integration that was created.
	/// </summary>
	///
	public DiscordIntegration Integration { get; internal set; }

	/// <summary>
	/// Gets the guild where the integration was created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildIntegrationCreateEventArgs"/> class.
	/// </summary>
	internal GuildIntegrationCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
