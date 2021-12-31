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
using DisCatSharp.Entities;
using DisCatSharp.Interactivity.Enums;

namespace DisCatSharp.Interactivity.EventHandling
{
    /// <summary>
    /// The button pagination request.
    /// </summary>
    internal class ButtonPaginationRequest : IPaginationRequest
    {
        private int _index;
        private readonly List<Page> _pages = new();

        private readonly TaskCompletionSource<bool> _tcs = new();

        private readonly CancellationToken _token;
        private readonly DiscordUser _user;
        private readonly DiscordMessage _message;
        private readonly PaginationButtons _buttons;
        private readonly PaginationBehaviour _wrapBehavior;
        private readonly ButtonPaginationBehavior _behaviorBehavior;

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonPaginationRequest"/> class.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="User">The user.</param>
        /// <param name="Behavior">The behavior.</param>
        /// <param name="ButtonBehavior">The button behavior.</param>
        /// <param name="Buttons">The buttons.</param>
        /// <param name="Pages">The pages.</param>
        /// <param name="Token">The token.</param>
        public ButtonPaginationRequest(DiscordMessage Message, DiscordUser User,
            PaginationBehaviour Behavior, ButtonPaginationBehavior ButtonBehavior,
            PaginationButtons Buttons, IEnumerable<Page> Pages, CancellationToken Token)
        {
            this._user = User;
            this._token = Token;
            this._buttons = new(Buttons);
            this._message = Message;
            this._wrapBehavior = Behavior;
            this._behaviorBehavior = ButtonBehavior;
            this._pages.AddRange(Pages);

            this._token.Register(() => this._tcs.TrySetResult(false));
        }

        /// <summary>
        /// Gets the page count.
        /// </summary>
        public int PageCount => this._pages.Count;

        /// <summary>
        /// Gets the page.
        /// </summary>
        public Task<Page> GetPage()
        {
            var page = Task.FromResult(this._pages[this._index]);

            if (this.PageCount is 1)
            {
                this._buttons.SkipLeft.Disable();
                this._buttons.Left.Disable();
                this._buttons.Right.Disable();
                this._buttons.SkipRight.Disable();
                this._buttons.Stop.Enable();
                return page;
            }

            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
                return page;

            this._buttons.SkipLeft.Disabled = this._index < 2;

            this._buttons.Left.Disabled = this._index < 1;

            this._buttons.Right.Disabled = this._index >= this.PageCount - 1;

            this._buttons.SkipRight.Disabled = this._index >= this.PageCount - 2;

            return page;
        }

        /// <summary>
        /// Skips the left.
        /// </summary>
        public Task SkipLeft()
        {
            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                this._index = this._index is 0 ? this._pages.Count - 1 : 0;
                return Task.CompletedTask;
            }

            this._index = 0;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Skips the right.
        /// </summary>
        public Task SkipRight()
        {
            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                this._index = this._index == this.PageCount - 1 ? 0 : this.PageCount - 1;
                return Task.CompletedTask;
            }

            this._index = this._pages.Count - 1;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the next page.
        /// </summary>
        public Task NextPage()
        {
            this._index++;

            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                if (this._index >= this.PageCount)
                    this._index = 0;

                return Task.CompletedTask;
            }

            this._index = Math.Min(this._index, this.PageCount - 1);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the previous page.
        /// </summary>
        public Task PreviousPage()
        {
            this._index--;

            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                if (this._index is -1)
                    this._index = this._pages.Count - 1;

                return Task.CompletedTask;
            }

            this._index = Math.Max(this._index, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the emojis.
        /// </summary>
        public Task<PaginationEmojis> GetEmojis() => Task.FromException<PaginationEmojis>(new NotSupportedException("Emojis aren't supported for this request."));

        /// <summary>
        /// Gets the buttons.
        /// </summary>
        public Task<IEnumerable<DiscordButtonComponent>> GetButtons()
            => Task.FromResult((IEnumerable<DiscordButtonComponent>)this._buttons.ButtonArray);

        /// <summary>
        /// Gets the message.
        /// </summary>
        public Task<DiscordMessage> GetMessage() => Task.FromResult(this._message);

        /// <summary>
        /// Gets the user.
        /// </summary>
        public Task<DiscordUser> GetUser() => Task.FromResult(this._user);

        /// <summary>
        /// Gets the task completion source.
        /// </summary>
        public Task<TaskCompletionSource<bool>> GetTaskCompletionSource() => Task.FromResult(this._tcs);

        /// <summary>
        /// Does the cleanup.
        /// </summary>
        public async Task DoCleanup()
        {
            switch (this._behaviorBehavior)
            {
                case ButtonPaginationBehavior.Disable:
                    var buttons = this._buttons.ButtonArray.Select(B => B.Disable());

                    var builder = new DiscordMessageBuilder()
                        .WithContent(this._pages[this._index].Content)
                        .AddEmbed(this._pages[this._index].Embed)
                        .AddComponents(buttons);

                    await builder.Modify(this._message).ConfigureAwait(false);
                    break;

                case ButtonPaginationBehavior.DeleteButtons:
                    builder = new DiscordMessageBuilder()
                        .WithContent(this._pages[this._index].Content)
                        .AddEmbed(this._pages[this._index].Embed);

                    await builder.Modify(this._message).ConfigureAwait(false);
                    break;

                case ButtonPaginationBehavior.DeleteMessage:
                    await this._message.Delete().ConfigureAwait(false);
                    break;

                case ButtonPaginationBehavior.Ignore:
                    break;
            }
        }
    }
}
