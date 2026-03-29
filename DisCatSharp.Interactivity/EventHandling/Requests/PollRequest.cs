using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     The poll request.
/// </summary>
public class PollRequest
{
	internal ConcurrentHashSet<PollEmoji> Collected;
	internal CancellationTokenSource Ct;
	internal List<DiscordEmoji> Emojis;
	internal DiscordMessage Message;
	internal TaskCompletionSource<bool> Tcs;
	internal TimeSpan Timeout;

	/// <summary>
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
		this.Emojis = [.. emojis];
		this.Collected = [];
		this.Message = message;

		foreach (var e in emojis)
			this.Collected.Add(new(e));
	}

	/// <summary>
	///     Clears the collected.
	/// </summary>
	internal void ClearCollected()
	{
		this.Collected.Clear();
		foreach (var e in this.Emojis)
			this.Collected.Add(new(e));
	}

	private readonly object _collectedLock = new();

	/// <summary>
	///     Removes the reaction.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	/// <param name="member">The member.</param>
	internal void RemoveReaction(DiscordEmoji emoji, DiscordUser member)
	{
		lock (this._collectedLock)
		{
			var e = this.Collected.FirstOrDefault(x => x.Emoji == emoji);
			if (e is null)
				return;

			if (!e.Voted.Contains(member))
				return;

			this.Collected.TryRemove(e);
			e.Voted.TryRemove(member);
			this.Collected.Add(e);
		}
	}

	/// <summary>
	///     Adds the reaction.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	/// <param name="member">The member.</param>
	internal void AddReaction(DiscordEmoji emoji, DiscordUser member)
	{
		lock (this._collectedLock)
		{
			var e = this.Collected.FirstOrDefault(x => x.Emoji == emoji);
			if (e is null)
				return;

			if (this.Collected.Any(x => x.Voted.Contains(member)))
				return;

			this.Collected.TryRemove(e);
			e.Voted.Add(member);
			this.Collected.Add(e);
		}
	}

	private bool _disposed;

	~PollRequest()
	{
		this.Dispose();
	}

	/// <summary>
	///     Disposes this PollRequest.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;
		this.Ct.Dispose();
		this.Tcs = null;
		GC.SuppressFinalize(this);
	}
}

/// <summary>
///     The poll emoji.
/// </summary>
public class PollEmoji
{
	public DiscordEmoji Emoji;
	public ConcurrentHashSet<DiscordUser> Voted;

	/// <summary>
	///     Initializes a new instance of the <see cref="PollEmoji" /> class.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	internal PollEmoji(DiscordEmoji emoji)
	{
		this.Emoji = emoji;
		this.Voted = [];
	}

	/// <summary>
	///     Gets the total.
	/// </summary>
	public int Total => this.Voted.Count;
}
