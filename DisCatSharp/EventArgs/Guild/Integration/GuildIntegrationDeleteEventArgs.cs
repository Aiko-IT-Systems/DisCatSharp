using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildIntegrationDeleted"/> event.
/// </summary>
public class GuildIntegrationDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the integration id which where deleted.
	/// </summary>
	///
	public ulong IntegrationId { get; internal set; }

	/// <summary>
	/// Gets the guild where the integration which where deleted.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the application id of the integration which where deleted.
	/// </summary>
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildIntegrationDeleteEventArgs"/> class.
	/// </summary>
	internal GuildIntegrationDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
