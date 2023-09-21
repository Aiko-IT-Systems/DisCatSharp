using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildIntegrationUpdated"/> event.
/// </summary>
public class GuildIntegrationUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the integration that was updated.
	/// </summary>
	///
	public DiscordIntegration Integration { get; internal set; }

	/// <summary>
	/// Gets the guild where the integration was updated.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildIntegrationUpdateEventArgs"/> class.
	/// </summary>
	internal GuildIntegrationUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
