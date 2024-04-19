using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessagePollVoteRemoved"/>
/// </summary>
public sealed class MessagePollVoteRemoveEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild of the poll, if it was send in a guild.
	/// </summary>
	public DiscordGuild? Guild { get; internal set; }

	/// <summary>
	/// Gets the guild id of the poll, if it was send in a guild.
	/// </summary>
	public ulong? GuildId { get; internal set; }

	/// <summary>
	/// Gets the channel of the poll, if it's in the cache.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the channel id of the poll.
	/// </summary>
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the message of the poll, if it's in the cache.
	/// </summary>
	public DiscordMessage Message { get; internal set; }

	/// <summary>
	/// Gets the message id of the poll.
	/// </summary>
	public ulong MessageId { get; internal set; }

	/// <summary>
	/// Gets the user who removed their vote.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the user id who removed their vote.
	/// </summary>
	public ulong UserId { get; internal set; }

	/// <summary>
	/// Gets the answer id.
	/// </summary>
	public int AnswerId { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MessagePollVoteRemoveEventArgs"/> class.
	/// </summary>
	internal MessagePollVoteRemoveEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
