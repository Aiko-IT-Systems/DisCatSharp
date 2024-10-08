using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.MessageReactionRemovedEmoji" />
/// </summary>
public sealed class MessageReactionRemoveEmojiEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="MessageReactionRemoveEmojiEventArgs" /> class.
	/// </summary>
	internal MessageReactionRemoveEmojiEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the channel the removed reactions were in.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	///     Gets the guild the removed reactions were in.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the message that had the removed reactions.
	/// </summary>
	public DiscordMessage Message { get; internal set; }

	/// <summary>
	///     Gets the emoji of the reaction that was removed.
	/// </summary>
	public DiscordEmoji Emoji { get; internal set; }
}
