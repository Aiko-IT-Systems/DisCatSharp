using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.MessageUpdated" /> event.
/// </summary>
public class MessageUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="MessageUpdateEventArgs" /> class.
	/// </summary>
	internal MessageUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the message that was updated.
	/// </summary>
	public DiscordMessage Message { get; internal set; }

	/// <summary>
	///     Gets the message before it got updated. This property will be null if the message was not cached.
	/// </summary>
	public DiscordMessage MessageBefore { get; internal set; }

	/// <summary>
	///     Gets the channel this message belongs to.
	/// </summary>
	public DiscordChannel Channel
		=> this.Message.Channel;

	/// <summary>
	///     Gets the guild this message belongs to.
	/// </summary>
	public DiscordGuild Guild
		=> this.Channel.Guild;

	/// <summary>
	///     Gets the guild id in case it couldn't convert.
	/// </summary>
	public ulong? GuildId
		=> this.Message.GuildId;

	/// <summary>
	///     Gets the author of the message.
	/// </summary>
	public DiscordUser Author
		=> this.Message.Author;

	/// <summary>
	///     Gets the collection of mentioned users.
	/// </summary>
	public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }

	/// <summary>
	///     Gets the collection of mentioned roles.
	/// </summary>
	public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }

	/// <summary>
	///     Gets the collection of mentioned channels.
	/// </summary>
	public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }
}
