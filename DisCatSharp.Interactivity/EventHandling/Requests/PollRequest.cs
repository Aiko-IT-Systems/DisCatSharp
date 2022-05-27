// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
	internal CancellationTokenSource Ct;
	internal TimeSpan Timeout;
	internal ConcurrentHashSet<PollEmoji> Collected;
	internal DiscordMessage Message;
	internal IEnumerable<DiscordEmoji> Emojis;

	/// <summary>
	///
	/// </summary>
	/// <param name="message"></param>
	/// <param name="timeout"></param>
	/// <param name="emojis"></param>
	public PollRequest(DiscordMessage message, TimeSpan timeout, IEnumerable<DiscordEmoji> emojis)
	{
		this.Tcs = new TaskCompletionSource<bool>();
		this.Ct = new CancellationTokenSource(timeout);
		this.Ct.Token.Register(() => this.Tcs.TrySetResult(true));
		this.Timeout = timeout;
		this.Emojis = emojis;
		this.Collected = new ConcurrentHashSet<PollEmoji>();
		this.Message = message;

		foreach (var e in emojis)
		{
			this.Collected.Add(new PollEmoji(e));
		}
	}

	/// <summary>
	/// Clears the collected.
	/// </summary>
	internal void ClearCollected()
	{
		this.Collected.Clear();
		foreach (var e in this.Emojis)
		{
			this.Collected.Add(new PollEmoji(e));
		}
	}

	/// <summary>
	/// Removes the reaction.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	/// <param name="member">The member.</param>
	internal void RemoveReaction(DiscordEmoji emoji, DiscordUser member)
	{
		if (this.Collected.Any(x => x.Emoji == emoji))
		{
			if (this.Collected.Any(x => x.Voted.Contains(member)))
			{
				var e = this.Collected.First(x => x.Emoji == emoji);
				this.Collected.TryRemove(e);
				e.Voted.TryRemove(member);
				this.Collected.Add(e);
			}
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
		{
			if (!this.Collected.Any(x => x.Voted.Contains(member)))
			{
				var e = this.Collected.First(x => x.Emoji == emoji);
				this.Collected.TryRemove(e);
				e.Voted.Add(member);
				this.Collected.Add(e);
			}
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
		this.Voted = new ConcurrentHashSet<DiscordUser>();
	}

	public DiscordEmoji Emoji;
	public ConcurrentHashSet<DiscordUser> Voted;
	/// <summary>
	/// Gets the total.
	/// </summary>
	public int Total => this.Voted.Count;
}
