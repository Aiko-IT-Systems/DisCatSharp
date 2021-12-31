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
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.Interactivity.Enums;

namespace DisCatSharp.Interactivity.EventHandling
{
    /// <summary>
    /// The pagination request.
    /// </summary>
    internal class PaginationRequest : IPaginationRequest
    {
        private TaskCompletionSource<bool> _tcs;
        private readonly CancellationTokenSource _ct;
        private readonly TimeSpan _timeout;
        private readonly List<Page> _pages;
        private readonly PaginationBehaviour _behaviour;
        private readonly DiscordMessage _message;
        private readonly PaginationEmojis _emojis;
        private readonly DiscordUser _user;
        private int _index = 0;

        /// <summary>
        /// Creates a new Pagination request
        /// </summary>
        /// <param name="Message">Message to paginate</param>
        /// <param name="User">User to allow control for</param>
        /// <param name="Behaviour">Behaviour during pagination</param>
        /// <param name="Deletion">Behavior on pagination end</param>
        /// <param name="Emojis">Emojis for this pagination object</param>
        /// <param name="Timeout">Timeout time</param>
        /// <param name="Pages">Pagination pages</param>
        internal PaginationRequest(DiscordMessage Message, DiscordUser User, PaginationBehaviour Behaviour, PaginationDeletion Deletion,
            PaginationEmojis Emojis, TimeSpan Timeout, params Page[] Pages)
        {
            this._tcs = new();
            this._ct = new(Timeout);
            this._ct.Token.Register(() => this._tcs.TrySetResult(true));
            this._timeout = Timeout;

            this._message = Message;
            this._user = User;

            this.PaginationDeletion = Deletion;
            this._behaviour = Behaviour;
            this._emojis = Emojis;

            this._pages = new List<Page>();
            foreach (var p in Pages)
            {
                this._pages.Add(p);
            }
        }

        /// <summary>
        /// Gets the page count.
        /// </summary>
        public int PageCount => this._pages.Count;

        /// <summary>
        /// Gets the pagination deletion.
        /// </summary>
        public PaginationDeletion PaginationDeletion { get; }

        /// <summary>
        /// Gets the page async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task<Page> GetPage()
        {
            await Task.Yield();

            return this._pages[this._index];
        }

        /// <summary>
        /// Skips the left async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task SkipLeft()
        {
            await Task.Yield();

            this._index = 0;
        }

        /// <summary>
        /// Skips the right async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task SkipRight()
        {
            await Task.Yield();

            this._index = this._pages.Count - 1;
        }

        /// <summary>
        /// Nexts the page async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task NextPage()
        {
            await Task.Yield();

            switch (this._behaviour)
            {
                case PaginationBehaviour.Ignore:
                    if (this._index == this._pages.Count - 1)
                        break;
                    else
                        this._index++;

                    break;

                case PaginationBehaviour.WrapAround:
                    if (this._index == this._pages.Count - 1)
                        this._index = 0;
                    else
                        this._index++;

                    break;
            }
        }

        /// <summary>
        /// Previous the page async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task PreviousPage()
        {
            await Task.Yield();

            switch (this._behaviour)
            {
                case PaginationBehaviour.Ignore:
                    if (this._index == 0)
                        break;
                    else
                        this._index--;

                    break;

                case PaginationBehaviour.WrapAround:
                    if (this._index == 0)
                        this._index = this._pages.Count - 1;
                    else
                        this._index--;

                    break;
            }
        }

        /// <summary>
        /// Gets the buttons async.
        /// </summary>
        /// <returns><see cref="System.NotSupportedException"/></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IEnumerable<DiscordButtonComponent>> GetButtons()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            => throw new NotSupportedException("This request does not support buttons.");

        /// <summary>
        /// Gets the emojis async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task<PaginationEmojis> GetEmojis()
        {
            await Task.Yield();

            return this._emojis;
        }

        /// <summary>
        /// Gets the message async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task<DiscordMessage> GetMessage()
        {
            await Task.Yield();

            return this._message;
        }

        /// <summary>
        /// Gets the user async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task<DiscordUser> GetUser()
        {
            await Task.Yield();

            return this._user;
        }

        /// <summary>
        /// Dos the cleanup async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task DoCleanup()
        {
            switch (this.PaginationDeletion)
            {
                case PaginationDeletion.DeleteEmojis:
                    await this._message.DeleteAllReactions().ConfigureAwait(false);
                    break;

                case PaginationDeletion.DeleteMessage:
                    await this._message.Delete().ConfigureAwait(false);
                    break;

                case PaginationDeletion.KeepEmojis:
                    break;
            }
        }

        /// <summary>
        /// Gets the task completion source async.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSource()
        {
            await Task.Yield();

            return this._tcs;
        }

        ~PaginationRequest()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this PaginationRequest.
        /// </summary>
        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null;
        }
    }
}

namespace DisCatSharp.Interactivity
{
    /// <summary>
    /// The pagination emojis.
    /// </summary>
    public class PaginationEmojis
    {
        public DiscordEmoji SkipLeft;
        public DiscordEmoji SkipRight;
        public DiscordEmoji Left;
        public DiscordEmoji Right;
        public DiscordEmoji Stop;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationEmojis"/> class.
        /// </summary>
        public PaginationEmojis()
        {
            this.Left = DiscordEmoji.FromUnicode("◀");
            this.Right = DiscordEmoji.FromUnicode("▶");
            this.SkipLeft = DiscordEmoji.FromUnicode("⏮");
            this.SkipRight = DiscordEmoji.FromUnicode("⏭");
            this.Stop = DiscordEmoji.FromUnicode("⏹");
        }
    }

    /// <summary>
    /// The page.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Gets or sets the embed.
        /// </summary>
        public DiscordEmbed Embed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="Content">The content.</param>
        /// <param name="Embed">The embed.</param>
        public Page(string Content = "", DiscordEmbedBuilder Embed = null)
        {
            this.Content = Content;
            this.Embed = Embed?.Build();
        }
    }
}
