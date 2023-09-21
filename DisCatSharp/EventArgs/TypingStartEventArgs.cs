using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.TypingStarted"/> event.
/// </summary>
public class TypingStartEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the channel in which the indicator was triggered.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the user that started typing.
	/// <para>This can be cast to a <see cref="DisCatSharp.Entities.DiscordMember"/> if the typing occurred in a guild.</para>
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the guild in which the indicator was triggered.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the date and time at which the user started typing.
	/// </summary>
	public DateTimeOffset StartedAt { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TypingStartEventArgs"/> class.
	/// </summary>
	internal TypingStartEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
