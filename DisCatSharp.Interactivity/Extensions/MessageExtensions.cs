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
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;

namespace DisCatSharp.Interactivity.Extensions
{
    /// <summary>
    /// Interactivity extension methods for <see cref="DisCatSharp.Entities.DiscordMessage"/>.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Waits for the next message that has the same author and channel as this message.
        /// </summary>
        /// <param name="Message">Original message.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessage(this DiscordMessage Message, TimeSpan? TimeoutOverride = null)
            => Message.Channel.GetNextMessage(Message.Author, TimeoutOverride);

        /// <summary>
        /// Waits for the next message with the same author and channel as this message, which also satisfies a predicate.
        /// </summary>
        /// <param name="Message">Original message.</param>
        /// <param name="Predicate">A predicate that should return <see langword="true"/> if a message matches.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessage(this DiscordMessage Message, Func<DiscordMessage, bool> Predicate, TimeSpan? TimeoutOverride = null)
            => Message.Channel.GetNextMessage(Msg => Msg.Author.Id == Message.Author.Id && Message.ChannelId == Msg.ChannelId && Predicate(Msg), TimeoutOverride);
        /// <summary>
        /// Waits for any button to be pressed on the specified message.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message)
            => GetInteractivity(Message).WaitForButton(Message);

        /// <summary>
        /// Waits for any button to be pressed on the specified message.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForButton(Message, TimeoutOverride);
        /// <summary>
        /// Waits for any button to be pressed on the specified message.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, CancellationToken Token)
            => GetInteractivity(Message).WaitForButtonAsync(Message, Token);
        /// <summary>
        /// Waits for a button with the specified Id to be pressed on the specified message.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Id">The Id of the button to wait for.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, string Id, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForButton(Message, Id, TimeoutOverride);

        /// <summary>
        /// Waits for a button with the specified Id to be pressed on the specified message.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Id">The Id of the button to wait for.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, string Id, CancellationToken Token)
            => GetInteractivity(Message).WaitForButtonAsync(Message, Id, Token);

        /// <summary>
        /// Waits for any button to be pressed on the specified message by the specified user.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="User">The user to wait for button input from.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, DiscordUser User, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForButton(Message, User, TimeoutOverride);

        /// <summary>
        /// Waits for any button to be pressed on the specified message by the specified user.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="User">The user to wait for button input from.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, DiscordUser User, CancellationToken Token)
            => GetInteractivity(Message).WaitForButtonAsync(Message, User, Token);

        /// <summary>
        /// Waits for any button to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Predicate">The predicate to filter interactions by.</param>
        /// <param name="TimeoutOverride">Override the timeout specified in <see cref="InteractivityConfiguration"/></param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForButton(Message, Predicate, TimeoutOverride);

        /// <summary>
        /// Waits for any button to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Predicate">The predicate to filter interactions by.</param>
        /// <param name="Token">A token to cancel interactivity with at any time. Pass <see cref="System.Threading.CancellationToken.None"/> to wait indefinitely.</param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(this DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, CancellationToken Token)
            => GetInteractivity(Message).WaitForButtonAsync(Message, Predicate, Token);

        /// <summary>
        /// Waits for any dropdown to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait for.</param>
        /// <param name="Predicate">A filter predicate.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <exception cref="System.ArgumentException">Thrown when the message doesn't contain any dropdowns</exception>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(this DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForSelect(Message, Predicate, TimeoutOverride);


        /// <summary>
        /// Waits for any dropdown to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait for.</param>
        /// <param name="Predicate">A filter predicate.</param>
        /// <param name="Token">A token that can be used to cancel interactivity. Pass <see cref="System.Threading.CancellationToken.None"/> to wait indefinitely.</param>
        /// <exception cref="System.ArgumentException">Thrown when the message doesn't contain any dropdowns</exception>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(this DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, CancellationToken Token)
            => GetInteractivity(Message).WaitForSelectAsync(Message, Predicate, Token);

        /// <summary>
        /// Waits for a dropdown to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Id">The Id of the dropdown to wait for.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(this DiscordMessage Message, string Id, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForSelect(Message, Id, TimeoutOverride);

        /// <summary>
        /// Waits for a dropdown to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Id">The Id of the dropdown to wait for.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(this DiscordMessage Message, string Id, CancellationToken Token)
            => GetInteractivity(Message).WaitForSelectAsync(Message, Id, Token);

        /// <summary>
        /// Waits for a dropdown to be interacted with by the specified user.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="User">The user to wait for.</param>
        /// <param name="Id">The Id of the dropdown to wait for.</param>
        /// <param name="TimeoutOverride"></param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(this DiscordMessage Message, DiscordUser User, string Id, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForSelect(Message, User, Id, TimeoutOverride);

        /// <summary>
        /// Waits for a dropdown to be interacted with by the specified user.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="User">The user to wait for.</param>
        /// <param name="Id">The Id of the dropdown to wait for.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(this DiscordMessage Message, DiscordUser User, string Id, CancellationToken Token)
   => GetInteractivity(Message).WaitForSelectAsync(Message, User, Id, Token);

        /// <summary>
        /// Waits for a reaction on this message from a specific user.
        /// </summary>
        /// <param name="Message">Target message.</param>
        /// <param name="User">The target user.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
        public static Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReaction(this DiscordMessage Message, DiscordUser User, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForReactionAsync(Message, User, TimeoutOverride);

        /// <summary>
        /// Waits for a specific reaction on this message from the specified user.
        /// </summary>
        /// <param name="Message">Target message.</param>
        /// <param name="User">The target user.</param>
        /// <param name="Emoji">The target emoji.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
        public static Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReaction(this DiscordMessage Message, DiscordUser User, DiscordEmoji Emoji, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).WaitForReactionAsync(E => E.Emoji == Emoji, Message, User, TimeoutOverride);

        /// <summary>
        /// Collects all reactions on this message within the timeout duration.
        /// </summary>
        /// <param name="Message">The message to collect reactions from.</param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
        public static Task<ReadOnlyCollection<Reaction>> CollectReactions(this DiscordMessage Message, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).CollectReactionsAsync(Message, TimeoutOverride);


        /// <summary>
        /// Begins a poll using this message.
        /// </summary>
        /// <param name="Message">Target message.</param>
        /// <param name="Emojis">Options for this poll.</param>
        /// <param name="BehaviorOverride">Overrides the action set in <see cref="InteractivityConfiguration.PaginationBehaviour"/></param>
        /// <param name="TimeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
        public static Task<ReadOnlyCollection<PollEmoji>> DoPoll(this DiscordMessage Message, IEnumerable<DiscordEmoji> Emojis, PollBehaviour? BehaviorOverride = null, TimeSpan? TimeoutOverride = null)
            => GetInteractivity(Message).DoPollAsync(Message, Emojis, BehaviorOverride, TimeoutOverride);

        /// <summary>
        /// Retrieves an interactivity instance from a message instance.
        /// </summary>
        internal static InteractivityExtension GetInteractivity(DiscordMessage Message)
        {
            var client = (DiscordClient)Message.Discord;
            var interactivity = client.GetInteractivity();

            return interactivity ?? throw new InvalidOperationException($"Interactivity is not enabled for this {(client._isShard ? "shard" : "client")}.");
        }
    }
}
