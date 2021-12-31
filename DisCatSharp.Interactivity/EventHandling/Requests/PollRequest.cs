// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp.Interactivity.EventHandling
{
    /// <summary>
    /// The poll request.
    /// </summary>
    public class PollRequest
    {
        internal TaskCompletionSource<bool> _tcs;
        internal CancellationTokenSource _ct;
        internal TimeSpan _timeout;
        internal ConcurrentHashSet<PollEmoji> _collected;
        internal DiscordMessage _message;
        internal IEnumerable<DiscordEmoji> _emojis;

        /// <summary>
        ///
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Timeout"></param>
        /// <param name="Emojis"></param>
        public PollRequest(DiscordMessage Message, TimeSpan Timeout, IEnumerable<DiscordEmoji> Emojis)
        {
            this._tcs = new TaskCompletionSource<bool>();
            this._ct = new CancellationTokenSource(Timeout);
            this._ct.Token.Register(() => this._tcs.TrySetResult(true));
            this._timeout = Timeout;
            this._emojis = Emojis;
            this._collected = new ConcurrentHashSet<PollEmoji>();
            this._message = Message;

            foreach (var e in Emojis)
            {
                this._collected.Add(new PollEmoji(e));
            }
        }

        /// <summary>
        /// Clears the collected.
        /// </summary>
        internal void ClearCollected()
        {
            this._collected.Clear();
            foreach (var e in this._emojis)
            {
                this._collected.Add(new PollEmoji(e));
            }
        }

        /// <summary>
        /// Removes the reaction.
        /// </summary>
        /// <param name="Emoji">The emoji.</param>
        /// <param name="Member">The member.</param>
        internal void RemoveReaction(DiscordEmoji Emoji, DiscordUser Member)
        {
            if (this._collected.Any(X => X.Emoji == Emoji))
            {
                if (this._collected.Any(X => X.Voted.Contains(Member)))
                {
                    var e = this._collected.First(X => X.Emoji == Emoji);
                    this._collected.TryRemove(e);
                    e.Voted.TryRemove(Member);
                    this._collected.Add(e);
                }
            }
        }

        /// <summary>
        /// Adds the reaction.
        /// </summary>
        /// <param name="Emoji">The emoji.</param>
        /// <param name="Member">The member.</param>
        internal void AddReaction(DiscordEmoji Emoji, DiscordUser Member)
        {
            if (this._collected.Any(X => X.Emoji == Emoji))
            {
                if (!this._collected.Any(X => X.Voted.Contains(Member)))
                {
                    var e = this._collected.First(X => X.Emoji == Emoji);
                    this._collected.TryRemove(e);
                    e.Voted.Add(Member);
                    this._collected.Add(e);
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
            this._ct.Dispose();
            this._tcs = null;
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
        /// <param name="Emoji">The emoji.</param>
        internal PollEmoji(DiscordEmoji Emoji)
        {
            this.Emoji = Emoji;
            this.Voted = new ConcurrentHashSet<DiscordUser>();
        }

        public DiscordEmoji Emoji;
        public ConcurrentHashSet<DiscordUser> Voted;
        /// <summary>
        /// Gets the total.
        /// </summary>
        public int Total => this.Voted.Count;
    }
}
