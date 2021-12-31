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
using System.Threading.Tasks;
using ConcurrentCollections;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling
{
    /// <summary>
    /// The paginator.
    /// </summary>
    internal class Paginator : IPaginator
    {
        DiscordClient _client;
        ConcurrentHashSet<IPaginationRequest> _requests;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="Client">Discord client</param>
        public Paginator(DiscordClient Client)
        {
            this._client = Client;
            this._requests = new ConcurrentHashSet<IPaginationRequest>();

            this._client.MessageReactionAdded += this.HandleReactionAdd;
            this._client.MessageReactionRemoved += this.HandleReactionRemove;
            this._client.MessageReactionsCleared += this.HandleReactionClear;
        }

        /// <summary>
        /// Dos the pagination async.
        /// </summary>
        /// <param name="Request">The request.</param>
        public async Task DoPagination(IPaginationRequest Request)
        {
            await this.ResetReactions(Request).ConfigureAwait(false);
            this._requests.Add(Request);
            try
            {
                var tcs = await Request.GetTaskCompletionSource().ConfigureAwait(false);
                await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
            }
            finally
            {
                this._requests.TryRemove(Request);
                try
                {
                    await Request.DoCleanup().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
                }
            }
        }

        /// <summary>
        /// Handles the reaction add.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Eventargs">The eventargs.</param>
        private Task HandleReactionAdd(DiscordClient Client, MessageReactionAddEventArgs Eventargs)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                foreach (var req in this._requests)
                {
                    var emojis = await req.GetEmojis().ConfigureAwait(false);
                    var msg = await req.GetMessage().ConfigureAwait(false);
                    var usr = await req.GetUser().ConfigureAwait(false);

                    if (msg.Id == Eventargs.Message.Id)
                    {
                        if (Eventargs.User.Id == usr.Id)
                        {
                            if (req.PageCount > 1 &&
                                (Eventargs.Emoji == emojis.Left ||
                                 Eventargs.Emoji == emojis.SkipLeft ||
                                 Eventargs.Emoji == emojis.Right ||
                                 Eventargs.Emoji == emojis.SkipRight ||
                                 Eventargs.Emoji == emojis.Stop))
                            {
                                await this.Paginate(req, Eventargs.Emoji).ConfigureAwait(false);
                            }
                            else if (Eventargs.Emoji == emojis.Stop &&
                                     req is PaginationRequest paginationRequest &&
                                     paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                            {
                                await this.Paginate(req, Eventargs.Emoji).ConfigureAwait(false);
                            }
                            else
                            {
                                await msg.DeleteReaction(Eventargs.Emoji, Eventargs.User).ConfigureAwait(false);
                            }
                        }
                        else if (Eventargs.User.Id != this._client.CurrentUser.Id)
                        {
                            if (Eventargs.Emoji != emojis.Left &&
                               Eventargs.Emoji != emojis.SkipLeft &&
                               Eventargs.Emoji != emojis.Right &&
                               Eventargs.Emoji != emojis.SkipRight &&
                               Eventargs.Emoji != emojis.Stop)
                            {
                                await msg.DeleteReaction(Eventargs.Emoji, Eventargs.User).ConfigureAwait(false);
                            }
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the reaction remove.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Eventargs">The eventargs.</param>
        private Task HandleReactionRemove(DiscordClient Client, MessageReactionRemoveEventArgs Eventargs)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                foreach (var req in this._requests)
                {
                    var emojis = await req.GetEmojis().ConfigureAwait(false);
                    var msg = await req.GetMessage().ConfigureAwait(false);
                    var usr = await req.GetUser().ConfigureAwait(false);

                    if (msg.Id == Eventargs.Message.Id)
                    {
                        if (Eventargs.User.Id == usr.Id)
                        {
                            if (req.PageCount > 1 &&
                                (Eventargs.Emoji == emojis.Left ||
                                 Eventargs.Emoji == emojis.SkipLeft ||
                                 Eventargs.Emoji == emojis.Right ||
                                 Eventargs.Emoji == emojis.SkipRight ||
                                 Eventargs.Emoji == emojis.Stop))
                            {
                                await this.Paginate(req, Eventargs.Emoji).ConfigureAwait(false);
                            }
                            else if (Eventargs.Emoji == emojis.Stop &&
                                     req is PaginationRequest paginationRequest &&
                                     paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                            {
                                await this.Paginate(req, Eventargs.Emoji).ConfigureAwait(false);
                            }
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the reaction clear.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Eventargs">The eventargs.</param>
        private Task HandleReactionClear(DiscordClient Client, MessageReactionsClearEventArgs Eventargs)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                foreach (var req in this._requests)
                {
                    var msg = await req.GetMessage().ConfigureAwait(false);

                    if (msg.Id == Eventargs.Message.Id)
                    {
                        await this.ResetReactions(req).ConfigureAwait(false);
                    }
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Resets the reactions async.
        /// </summary>
        /// <param name="P">The p.</param>
        private async Task ResetReactions(IPaginationRequest P)
        {
            var msg = await P.GetMessage().ConfigureAwait(false);
            var emojis = await P.GetEmojis().ConfigureAwait(false);

            // Test permissions to avoid a 403:
            // https://totally-not.a-sketchy.site/3pXpRLK.png
            // Yes, this is an issue
            // No, we should not require people to guarantee MANAGE_MESSAGES
            // Need to check following:
            // - In guild?
            //  - If yes, check if have permission
            // - If all above fail (DM || guild && no permission), skip this
            var chn = msg.Channel;
            var gld = chn?.Guild;
            var mbr = gld?.CurrentMember;

            if (mbr != null /* == is guild and cache is valid */ && (chn.PermissionsFor(mbr) & Permissions.ManageChannels) != 0) /* == has permissions */
                await msg.DeleteAllReactions("Pagination").ConfigureAwait(false);
            // ENDOF: 403 fix

            if (P.PageCount > 1)
            {
                if (emojis.SkipLeft != null)
                    await msg.CreateReaction(emojis.SkipLeft).ConfigureAwait(false);
                if (emojis.Left != null)
                    await msg.CreateReaction(emojis.Left).ConfigureAwait(false);
                if (emojis.Right != null)
                    await msg.CreateReaction(emojis.Right).ConfigureAwait(false);
                if (emojis.SkipRight != null)
                    await msg.CreateReaction(emojis.SkipRight).ConfigureAwait(false);
                if (emojis.Stop != null)
                    await msg.CreateReaction(emojis.Stop).ConfigureAwait(false);
            }
            else if (emojis.Stop != null && P is PaginationRequest paginationRequest && paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
            {
                await msg.CreateReaction(emojis.Stop).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Paginates the async.
        /// </summary>
        /// <param name="P">The p.</param>
        /// <param name="Emoji">The emoji.</param>
        private async Task Paginate(IPaginationRequest P, DiscordEmoji Emoji)
        {
            var emojis = await P.GetEmojis().ConfigureAwait(false);
            var msg = await P.GetMessage().ConfigureAwait(false);

            if (Emoji == emojis.SkipLeft)
                await P.SkipLeft().ConfigureAwait(false);
            else if (Emoji == emojis.Left)
                await P.PreviousPage().ConfigureAwait(false);
            else if (Emoji == emojis.Right)
                await P.NextPage().ConfigureAwait(false);
            else if (Emoji == emojis.SkipRight)
                await P.SkipRight().ConfigureAwait(false);
            else if (Emoji == emojis.Stop)
            {
                var tcs = await P.GetTaskCompletionSource().ConfigureAwait(false);
                tcs.TrySetResult(true);
                return;
            }

            var page = await P.GetPage().ConfigureAwait(false);
            var builder = new DiscordMessageBuilder()
                .WithContent(page.Content)
                .WithEmbed(page.Embed);

            await builder.Modify(msg).ConfigureAwait(false);
        }

        ~Paginator()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this EventWaiter
        /// </summary>
        public void Dispose()
        {
            this._client.MessageReactionAdded -= this.HandleReactionAdd;
            this._client.MessageReactionRemoved -= this.HandleReactionRemove;
            this._client.MessageReactionsCleared -= this.HandleReactionClear;
            this._client = null;
            this._requests.Clear();
            this._requests = null;
        }
    }
}
