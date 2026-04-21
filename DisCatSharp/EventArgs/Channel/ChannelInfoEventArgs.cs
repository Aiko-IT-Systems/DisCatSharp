using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///    Represents arguments for <see cref="DiscordClient.ChannelInfo" /> event.
/// </summary>
public sealed class ChannelInfoEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ChannelInfoEventArgs" /> class.
	/// </summary>
	internal ChannelInfoEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	/// 	Gets the guild for which the channel information is being provided.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///		Gets the ID of the guild for which the channel information is being provided.
	/// </summary>
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// 	Gets the list of channels in the guild, along with their information such as ID, status, and voice start time.
	/// </summary>
	public List<DiscordChannelInfo> Channels { get; internal set; }
}
