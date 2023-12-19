using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildIntegrationsUpdated"/> event.
/// </summary>
public class GuildIntegrationsUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild that had its integrations updated.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildIntegrationsUpdateEventArgs"/> class.
	/// </summary>
	internal GuildIntegrationsUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
