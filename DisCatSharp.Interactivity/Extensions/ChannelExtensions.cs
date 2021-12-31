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
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;

namespace DisCatSharp.Interactivity.Extensions
{
    /// <summary>
    /// Interactivity extension methods for <see cref="DisCatSharp.Entities.DiscordChannel"/>.
    /// </summary>
    public static class ChannelExtensions
    {
        /// <summary>
        /// Waits for the next message sent in this channel that satisfies the predicate.
        /// </summary>
        /// <param name="Channel">The channel to monitor.</param>
        /// <param name="Predicate">A predicate that should return <see langword="true"/> if a message matches.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessage(this DiscordChannel Channel, Func<DiscordMessage, bool> Predicate, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Channel).WaitForMessageAsync(Msg => Msg.ChannelId == Channel.Id && Predicate(Msg), TimeoutOverride);

        /// <summary>
        /// Waits for the next message sent in this channel.
        /// </summary>
        /// <param name="Channel">The channel to monitor.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessage(this DiscordChannel Channel, TimeSpan? TimeoutOverride = null)
            => Channel.GetNextMessage(Msg => true, TimeoutOverride);

        /// <summary>
        /// Waits for the next message sent in this channel from a specific user.
        /// </summary>
        /// <param name="Channel">The channel to monitor.</param>
        /// <param name="User">The target user.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessage(this DiscordChannel Channel, DiscordUser User, TimeSpan? TimeoutOverride = null)
            => Channel.GetNextMessage(Msg => Msg.Author.Id == User.Id, TimeoutOverride);

        /// <summary>
        /// Waits for a specific user to start typing in this channel.
        /// </summary>
        /// <param name="Channel">The target channel.</param>
        /// <param name="User">The target user.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTyping(this DiscordChannel Channel, DiscordUser User, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Channel).WaitForUserTypingAsync(User, Channel, TimeoutOverride);


        /// <summary>
        /// Sends a new paginated message.
        /// </summary>
        /// <param name="Channel">Target channel.</param>
        /// <param name="User">The user that'll be able to control the pages.</param>
        /// <param name="Pages">A collection of <see cref="Page"/> to display.</param>
        /// <param name="Emojis">Pagination emojis.</param>
        /// <param name="Behaviour">Pagination behaviour (when hitting max and min indices).</param>
        /// <param name="Deletion">Deletion behaviour.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task SendPaginatedMessage(this DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationEmojis Emojis, PaginationBehaviour? Behaviour = default, PaginationDeletion? Deletion = default, TimeSpan? Timeoutoverride = null)
            => GetInteractivity(Channel).SendPaginatedMessageAsync(Channel, User, Pages, Emojis, Behaviour, Deletion, Timeoutoverride);

        /// <summary>
        /// Sends a new paginated message with buttons.
        /// </summary>
        /// <param name="Channel">Target channel.</param>
        /// <param name="User">The user that'll be able to control the pages.</param>
        /// <param name="Pages">A collection of <see cref="Page"/> to display.</param>
        /// <param name="Buttons">Pagination buttons (leave null to default to ones on configuration).</param>
        /// <param name="Behaviour">Pagination behaviour.</param>
        /// <param name="Deletion">Deletion behaviour</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task SendPaginatedMessage(this DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationButtons Buttons, PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default, CancellationToken Token = default)
            => GetInteractivity(Channel).SendPaginatedMessageAsync(Channel, User, Pages, Buttons, Behaviour, Deletion, Token);

        /// <inheritdoc cref="SendPaginatedMessage(DisCatSharp.Entities.DiscordChannel,DisCatSharp.Entities.DiscordUser,System.Collections.Generic.IEnumerable{DisCatSharp.Interactivity.Page},DisCatSharp.Interactivity.EventHandling.PaginationButtons,System.Nullable{DisCatSharp.Interactivity.Enums.PaginationBehaviour},System.Nullable{DisCatSharp.Interactivity.Enums.ButtonPaginationBehavior},System.Threading.CancellationToken)"/>
        public static Task SendPaginatedMessage(this DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default, CancellationToken Token = default)
            => Channel.SendPaginatedMessage(User, Pages, default, Behaviour, Deletion, Token);

        /// <summary>
        /// Sends a new paginated message with buttons.
        /// </summary>
        /// <param name="Channel">Target channel.</param>
        /// <param name="User">The user that'll be able to control the pages.</param>
        /// <param name="Pages">A collection of <see cref="Page"/> to display.</param>
        /// <param name="Buttons">Pagination buttons (leave null to default to ones on configuration).</param>
        /// <param name="Behaviour">Pagination behaviour.</param>
        /// <param name="Deletion">Deletion behaviour</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task SendPaginatedMessage(this DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationButtons Buttons, TimeSpan? Timeoutoverride, PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default)
            => GetInteractivity(Channel).SendPaginatedMessage(Channel, User, Pages, Buttons, Timeoutoverride, Behaviour, Deletion);


        /// <summary>
        /// Sends the paginated message async.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        /// <param name="User">The user.</param>
        /// <param name="Pages">The pages.</param>
        /// <param name="Timeoutoverride">The timeoutoverride.</param>
        /// <param name="Behaviour">The behaviour.</param>
        /// <param name="Deletion">The deletion.</param>
        /// <returns>A Task.</returns>
        public static Task SendPaginatedMessage(this DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, TimeSpan? Timeoutoverride, PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default)
            => Channel.SendPaginatedMessage(User, Pages, default, Timeoutoverride, Behaviour, Deletion);

        /// <summary>
        /// Retrieves an interactivity instance from a channel instance.
        /// </summary>
        private static InteractivityExtension GetInteractivity(DiscordChannel Channel)
        {
            var client = (DiscordClient)Channel.Discord;
            var interactivity = client.GetInteractivity();

            return interactivity ?? throw new InvalidOperationException($"Interactivity is not enabled for this {(client._isShard ? "shard" : "client")}.");
        }
    }
}
