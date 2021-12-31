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
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling
{
    /// <summary>
    /// The component paginator.
    /// </summary>
    internal class ComponentPaginator : IPaginator
    {
        private readonly DiscordClient _client;
        private readonly InteractivityConfiguration _config;
        private readonly DiscordMessageBuilder _builder = new();
        private readonly Dictionary<ulong, IPaginationRequest> _requests = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentPaginator"/> class.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Config">The config.</param>
        public ComponentPaginator(DiscordClient Client, InteractivityConfiguration Config)
        {
            this._client = Client;
            this._client.ComponentInteractionCreated += this.Handle;
            this._config = Config;
        }

        /// <summary>
        /// Does the pagination async.
        /// </summary>
        /// <param name="Request">The request.</param>
        public async Task DoPagination(IPaginationRequest Request)
        {
            var id = (await Request.GetMessage().ConfigureAwait(false)).Id;
            this._requests.Add(id, Request);

            try
            {
                var tcs = await Request.GetTaskCompletionSource().ConfigureAwait(false);
                await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "There was an exception while paginating.");
            }
            finally
            {
                this._requests.Remove(id);
                try
                {
                    await Request.DoCleanup().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "There was an exception while cleaning up pagination.");
                }
            }
        }

        /// <summary>
        /// Disposes the paginator.
        /// </summary>
        public void Dispose() => this._client.ComponentInteractionCreated -= this.Handle;


        /// <summary>
        /// Handles the pagination event.
        /// </summary>
        /// <param name="_">The client.</param>
        /// <param name="E">The event arguments.</param>
        private async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs E)
        {
            if (E.Interaction.Type == InteractionType.ModalSubmit)
                return;

            if (!this._requests.TryGetValue(E.Message.Id, out var req))
                return;

            if (this._config.AckPaginationButtons)
            {
                E.Handled = true;
                await E.Interaction.CreateResponse(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
            }

            if (await req.GetUser().ConfigureAwait(false) != E.User)
            {
                if (this._config.ResponseBehavior is InteractionResponseBehavior.Respond)
                    await E.Interaction.CreateFollowupMessageAsync(new() { Content = this._config.ResponseMessage, IsEphemeral = true }).ConfigureAwait(false);

                return;
            }

            if (req is InteractionPaginationRequest ipr)
                ipr.RegenerateCts(E.Interaction); // Necessary to ensure we don't prematurely yeet the CTS //

            await this.HandlePagination(req, E).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the pagination async.
        /// </summary>
        /// <param name="Request">The request.</param>
        /// <param name="Args">The arguments.</param>
        private async Task HandlePagination(IPaginationRequest Request, ComponentInteractionCreateEventArgs Args)
        {
            var buttons = this._config.PaginationButtons;
            var msg = await Request.GetMessage().ConfigureAwait(false);
            var id = Args.Id;
            var tcs = await Request.GetTaskCompletionSource().ConfigureAwait(false);

#pragma warning disable CS8846 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
            var paginationTask = id switch
#pragma warning restore CS8846 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
            {
                _ when id == buttons.SkipLeft.CustomId => Request.SkipLeft(),
                _ when id == buttons.SkipRight.CustomId => Request.SkipRight(),
                _ when id == buttons.Stop.CustomId => Task.FromResult(tcs.TrySetResult(true)),
                _ when id == buttons.Left.CustomId => Request.PreviousPage(),
                _ when id == buttons.Right.CustomId => Request.NextPage(),
            };

            await paginationTask.ConfigureAwait(false);

            if (id == buttons.Stop.CustomId)
                return;

            var page = await Request.GetPage().ConfigureAwait(false);


            var bts = await Request.GetButtons().ConfigureAwait(false);

            if (Request is InteractionPaginationRequest ipr)
            {
                var builder = new DiscordWebhookBuilder()
                    .WithContent(page.Content)
                    .AddEmbed(page.Embed)
                    .AddComponents(bts);

                await Args.Interaction.EditOriginalResponseAsync(builder).ConfigureAwait(false);
                return;
            }

            this._builder.Clear();

            this._builder
                .WithContent(page.Content)
                .AddEmbed(page.Embed)
                .AddComponents(bts);

            await this._builder.Modify(msg).ConfigureAwait(false);

        }
    }
}
