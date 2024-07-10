using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessagesBulkDeleted"/> event.
/// </summary>
public class MessageBulkDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets a collection of the deleted messages.
	/// </summary>
	public IReadOnlyList<DiscordMessage> Messages { get; internal set; }

	/// <summary>
	/// Gets the channel in which the deletion occurred.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the guild in which the deletion occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageBulkDeleteEventArgs"/> class.
	/// </summary>
	internal MessageBulkDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
