using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildEmojisUpdated"/> event.
/// </summary>
public class GuildEmojisUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the list of emojis after the change.
	/// </summary>
	public IReadOnlyDictionary<ulong, DiscordEmoji> EmojisAfter { get; internal set; }

	/// <summary>
	/// Gets the list of emojis before the change.
	/// </summary>
	public IReadOnlyDictionary<ulong, DiscordEmoji> EmojisBefore { get; internal set; }

	/// <summary>
	/// Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildEmojisUpdateEventArgs"/> class.
	/// </summary>
	internal GuildEmojisUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
