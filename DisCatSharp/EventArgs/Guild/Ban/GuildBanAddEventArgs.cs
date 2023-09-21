using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildBanAdded"/> event.
/// </summary>
public class GuildBanAddEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the member that was banned.
	/// </summary>
	public DiscordMember Member { get; internal set; }

	/// <summary>
	/// Gets the guild this member was banned in.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildBanAddEventArgs"/> class.
	/// </summary>
	internal GuildBanAddEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
