using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// The poll request.
/// </summary>
public class PollRequest
{
	internal TaskCompletionSource<bool> Tcs;
	internal readonly CancellationTokenSource Ct;
	internal TimeSpan Timeout;
	internal readonly ConcurrentHashSet<PollEmoji> Collected;
	internal readonly DiscordMessage Message;
	internal readonly List<DiscordEmoji> Emojis;

	/// <summary>
	///
	/// </summary>
	/// <param name="message"></param>
	/// <param name="timeout"></param>
	/// <param name="emojis"></param>
	public PollRequest(DiscordMessage message, TimeSpan timeout, IEnumerable<DiscordEmoji> emojis)
	{
		this.Tcs = new();
		this.Ct = new(timeout);
		this.Ct.Token.Register(() => this.Tcs.TrySetResult(true));
		this.Timeout = timeout;
		this.Emojis = emojis.ToList();
		this.Collected = new();
		this.Message = message;

		foreach (var e in emojis) this.Collected.Add(new(e));
	}

	/// <summary>
	/// Clears the collected.
	/// </summary>
	internal void ClearCollected()
	{
		this.Collected.Clear();
		foreach (var e in this.Emojis) this.Collected.Add(new(e));
	}

	/// <summary>
	/// Removes the reaction.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	/// <param name="member">The member.</param>
	internal void RemoveReaction(DiscordEmoji emoji, DiscordUser member)
	{
		if (this.Collected.Any(x => x.Emoji == emoji))
			if (this.Collected.Any(x => x.Voted.Contains(member)))
			{
				var e = this.Collected.First(x => x.Emoji == emoji);
				this.Collected.TryRemove(e);
				e.Voted.TryRemove(member);
				this.Collected.Add(e);
			}
	}

	/// <summary>
	/// Adds the reaction.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	/// <param name="member">The member.</param>
	internal void AddReaction(DiscordEmoji emoji, DiscordUser member)
	{
		if (this.Collected.Any(x => x.Emoji == emoji))
			if (!this.Collected.Any(x => x.Voted.Contains(member)))
			{
				var e = this.Collected.First(x => x.Emoji == emoji);
				this.Collected.TryRemove(e);
				e.Voted.Add(member);
				this.Collected.Add(e);
			}
	}

	~PollRequest()
	{
		this.Dispose();
	}

	/// <summary>
	/// Disposes this PollRequest.
	/// </summary>
	public void Dispose()
	{
		this.Ct.Dispose();
		this.Tcs = null;
	}
}

/// <summary>
/// The poll emoji.
/// </summary>
public class PollEmoji
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PollEmoji"/> class.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	internal PollEmoji(DiscordEmoji emoji)
	{
		this.Emoji = emoji;
		this.Voted = new();
	}

	public readonly DiscordEmoji Emoji;
	public readonly ConcurrentHashSet<DiscordUser> Voted;
	/// <summary>
	/// Gets the total.
	/// </summary>
	public int Total => this.Voted.Count;
}
