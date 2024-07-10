using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.EmbeddedActivityUpdated"/> event.
/// </summary>
public class EmbeddedActivityUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the channel.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the embedded activity.
	/// </summary>
	public DiscordActivity EmbeddedActivityBefore { get; internal set; }

	/// <summary>
	/// Gets the embedded activity.
	/// </summary>
	public DiscordActivity EmbeddedActivityAfter { get; internal set; }

	/// <summary>
	/// Gets the users in the activity.
	/// </summary>
	public IReadOnlyList<DiscordMember> Users { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="EmbeddedActivityUpdateEventArgs"/> class.
	/// </summary>
	internal EmbeddedActivityUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
