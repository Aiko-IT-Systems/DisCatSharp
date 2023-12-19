using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents event args for the <see cref="DiscordClient.GuildStickersUpdated"/> event.
/// </summary>
public class GuildStickersUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the list of stickers after the change.
	/// </summary>
	public IReadOnlyDictionary<ulong, DiscordSticker> StickersAfter { get; internal set; }

	/// <summary>
	/// Gets the list of stickers before the change.
	/// </summary>
	public IReadOnlyDictionary<ulong, DiscordSticker> StickersBefore { get; internal set; }

	/// <summary>
	/// Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildStickersUpdateEventArgs"/> class.
	/// </summary>
	internal GuildStickersUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
