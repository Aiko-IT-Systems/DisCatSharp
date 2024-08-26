using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.DmChannelDeleted" /> event.
/// </summary>
public class DmChannelDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DmChannelDeleteEventArgs" /> class.
	/// </summary>
	internal DmChannelDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the direct message channel that was deleted.
	/// </summary>
	public DiscordDmChannel Channel { get; internal set; }
}
