using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageCreated"/> event.
/// </summary>
public class MessageCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the message that was created.
	/// </summary>
	public DiscordMessage Message { get; internal set; }

	/// <summary>
	/// Gets the channel this message belongs to.
	/// </summary>
	public DiscordChannel Channel
		=> this.Message.Channel;

	/// <summary>
	/// Gets the guild this message belongs to.
	/// </summary>
	public DiscordGuild Guild
		=> this.Message.Guild;

	/// <summary>
	/// Gets the guild id in case it couldn't convert.
	/// </summary>
	public ulong? GuildId
		=> this.Message.GuildId;

	/// <summary>
	/// Gets the author of the message.
	/// </summary>
	public DiscordUser Author
		=> this.Message.Author;

	/// <summary>
	/// Gets the collection of mentioned users.
	/// </summary>
	public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }

	/// <summary>
	/// Gets the collection of mentioned roles.
	/// </summary>
	public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }

	/// <summary>
	/// Gets the collection of mentioned channels.
	/// </summary>
	public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageCreateEventArgs"/> class.
	/// </summary>
	internal MessageCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
