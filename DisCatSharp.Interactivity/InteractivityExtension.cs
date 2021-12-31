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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;

namespace DisCatSharp.Interactivity
{
    /// <summary>
    /// Extension class for DisCatSharp.Interactivity
    /// </summary>
    public class InteractivityExtension : BaseExtension
    {

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets the config.
        /// </summary>
        internal InteractivityConfiguration Config { get; }

        private EventWaiter<MessageCreateEventArgs> _messageCreatedWaiter;

        private EventWaiter<MessageReactionAddEventArgs> _messageReactionAddWaiter;

        private EventWaiter<TypingStartEventArgs> _typingStartWaiter;

        private EventWaiter<ComponentInteractionCreateEventArgs> _modalInteractionWaiter;

        private EventWaiter<ComponentInteractionCreateEventArgs> _componentInteractionWaiter;

        private ComponentEventWaiter _componentEventWaiter;

        private ModalEventWaiter _modalEventWaiter;

        private ReactionCollector _reactionCollector;

        private Poller _poller;

        private Paginator _paginator;
        private  ComponentPaginator _compPaginator;

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractivityExtension"/> class.
        /// </summary>
        /// <param name="Cfg">The configuration.</param>
        internal InteractivityExtension(InteractivityConfiguration Cfg)
        {
            this.Config = new InteractivityConfiguration(Cfg);
        }

        /// <summary>
        /// Setups the Interactivity Extension.
        /// </summary>
        /// <param name="Client">Discord client.</param>
        protected internal override void Setup(DiscordClient Client)
        {
            this.Client = Client;
            this._messageCreatedWaiter = new EventWaiter<MessageCreateEventArgs>(this.Client);
            this._messageReactionAddWaiter = new EventWaiter<MessageReactionAddEventArgs>(this.Client);
            this._componentInteractionWaiter = new EventWaiter<ComponentInteractionCreateEventArgs>(this.Client);
            this._modalInteractionWaiter = new EventWaiter<ComponentInteractionCreateEventArgs>(this.Client);
            this._typingStartWaiter = new EventWaiter<TypingStartEventArgs>(this.Client);
            this._poller = new Poller(this.Client);
            this._reactionCollector = new ReactionCollector(this.Client);
            this._paginator = new Paginator(this.Client);
            this._compPaginator = new(this.Client, this.Config);
            this._componentEventWaiter = new(this.Client, this.Config);
            this._modalEventWaiter = new(this.Client, this.Config);

        }

        /// <summary>
        /// Makes a poll and returns poll results.
        /// </summary>
        /// <param name="M">Message to create poll on.</param>
        /// <param name="Emojis">Emojis to use for this poll.</param>
        /// <param name="Behaviour">What to do when the poll ends.</param>
        /// <param name="Timeout">override timeout period.</param>
        /// <returns></returns>
        public async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(DiscordMessage M, IEnumerable<DiscordEmoji> Emojis, PollBehaviour? Behaviour = default, TimeSpan? Timeout = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            if (!Emojis.Any())
                throw new ArgumentException("You need to provide at least one emoji for a poll!");

            foreach (var em in Emojis)
                await M.CreateReaction(em).ConfigureAwait(false);

            var res = await this._poller.DoPollAsync(new PollRequest(M, Timeout ?? this.Config.Timeout, Emojis)).ConfigureAwait(false);

            var pollbehaviour = Behaviour ?? this.Config.PollBehaviour;
            var thismember = await M.Channel.Guild.GetMemberAsync(this.Client.CurrentUser.Id).ConfigureAwait(false);

            if (pollbehaviour == PollBehaviour.DeleteEmojis && M.Channel.PermissionsFor(thismember).HasPermission(Permissions.ManageMessages))
                await M.DeleteAllReactions().ConfigureAwait(false);

            return new ReadOnlyCollection<PollEmoji>(res.ToList());
        }

        /// <summary>
        /// Waits for any button in the specified collection to be pressed.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Buttons">A collection of buttons to listen for.</param>
        /// <param name="TimeoutOverride">Override the timeout period in <see cref="InteractivityConfiguration"/>.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(DiscordMessage Message, IEnumerable<DiscordButtonComponent> Buttons, TimeSpan? TimeoutOverride = null)
            => this.WaitForButtonAsync(Message, Buttons, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for any button in the specified collection to be pressed.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Buttons">A collection of buttons to listen for.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage Message, IEnumerable<DiscordButtonComponent> Buttons, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Buttons.Any())
                throw new ArgumentException("You must specify at least one button to listen for.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Button))
                throw new ArgumentException("Provided Message does not contain any button components.");

            var res = await this._componentEventWaiter
                .WaitForMatchAsync(new(Message,
                    C =>
                        C.Interaction.Data.ComponentType == ComponentType.Button &&
                        Buttons.Any(B => B.CustomId == C.Id), Token)).ConfigureAwait(false);

            return new(res is null, res);
        }

        /// <summary>
        /// Waits for a user modal submit.
        /// </summary>
        /// <param name="custom_id">The custom id of the modal to wait for.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of the modal.</returns>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForModal(string CustomId, TimeSpan? TimeoutOverride = null)
            => this.WaitForModalAsync(CustomId, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for a user modal submit.
        /// </summary>
        /// <param name="custom_id">The custom id of the modal to wait for.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of the modal.</returns>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForModalAsync(string CustomId, CancellationToken Token)
        {
            var result =
                await this
                ._modalEventWaiter
                .WaitForModalMatchAsync(new(CustomId, C => C.Interaction.Type == InteractionType.ModalSubmit, Token))
                .ConfigureAwait(false);

            return new(result is null, result);
        }

        /// <summary>
        /// Waits for any button on the specified message to be pressed.
        /// </summary>
        /// <param name="Message">The message to wait for the button on.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(DiscordMessage Message, TimeSpan? TimeoutOverride = null)
            => this.WaitForButtonAsync(Message, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for any button on the specified message to be pressed.
        /// </summary>
        /// <param name="Message">The message to wait for the button on.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage Message, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Button))
                throw new ArgumentException("Message does not contain any button components.");

            var ids = Message.Components.SelectMany(M => M.Components).Select(C => C.CustomId);

            var result =
                await this
                ._componentEventWaiter
                .WaitForMatchAsync(new(Message, C => C.Interaction.Data.ComponentType == ComponentType.Button && ids.Contains(C.Id), Token))
                .ConfigureAwait(false);

            return new(result is null, result);
        }

        /// <summary>
        /// Waits for any button on the specified message to be pressed by the specified user.
        /// </summary>
        /// <param name="Message">The message to wait for the button on.</param>
        /// <param name="User">The user to wait for the button press from.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(DiscordMessage Message, DiscordUser User, TimeSpan? TimeoutOverride = null)
            => this.WaitForButtonAsync(Message, User, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for any button on the specified message to be pressed by the specified user.
        /// </summary>
        /// <param name="Message">The message to wait for the button on.</param>
        /// <param name="User">The user to wait for the button press from.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage Message, DiscordUser User, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Button))
                throw new ArgumentException("Message does not contain any button components.");

            var result = await this
                ._componentEventWaiter
                .WaitForMatchAsync(new(Message, (C) => C.Interaction.Data.ComponentType is ComponentType.Button && C.User == User, Token))
                .ConfigureAwait(false);

            return new(result is null, result);

        }

        /// <summary>
        /// Waits for a button with the specified Id to be pressed.
        /// </summary>
        /// <param name="Message">The message to wait for the button on.</param>
        /// <param name="Id">The Id of the button to wait for.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of the operation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(DiscordMessage Message, string Id, TimeSpan? TimeoutOverride = null)
            => this.WaitForButtonAsync(Message, Id, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for a button with the specified Id to be pressed.
        /// </summary>
        /// <param name="Message">The message to wait for the button on.</param>
        /// <param name="Id">The Id of the button to wait for.</param>
        /// <param name="Token">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <returns>A <see cref="InteractivityResult{T}"/> with the result of the operation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage Message, string Id, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Button))
                throw new ArgumentException("Message does not contain any button components.");

            if (!Message.Components.SelectMany(C => C.Components).OfType<DiscordButtonComponent>().Any(C => C.CustomId == Id))
                throw new ArgumentException($"Message does not contain button with Id of '{Id}'.");
            var result = await this
                ._componentEventWaiter
                .WaitForMatchAsync(new(Message, (C) => C.Interaction.Data.ComponentType is ComponentType.Button && C.Id == Id, Token))
                .ConfigureAwait(false);

            return new(result is null, result);
        }

        /// <summary>
        /// Waits for any button to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Predicate">The predicate to filter interactions by.</param>
        /// <param name="TimeoutOverride">Override the timeout specified in <see cref="InteractivityConfiguration"/></param>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, TimeSpan? TimeoutOverride = null)
            => this.WaitForButtonAsync(Message, Predicate, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for any button to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Predicate">The predicate to filter interactions by.</param>
        /// <param name="Token">A token to cancel interactivity with at any time. Pass <see cref="System.Threading.CancellationToken.None"/> to wait indefinitely.</param>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Button))
                throw new ArgumentException("Message does not contain any button components.");

            var result = await this
                ._componentEventWaiter
                .WaitForMatchAsync(new(Message, C => C.Interaction.Data.ComponentType is ComponentType.Button && Predicate(C), Token))
                .ConfigureAwait(false);

            return new(result is null, result);
        }


        /// <summary>
        /// Waits for any dropdown to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait for.</param>
        /// <param name="Predicate">A filter predicate.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <exception cref="System.ArgumentException">Thrown when the Provided message does not contain any dropdowns</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, TimeSpan? TimeoutOverride = null)
            => this.WaitForSelectAsync(Message, Predicate, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for any dropdown to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait for.</param>
        /// <param name="Predicate">A filter predicate.</param>
        /// <param name="Token">A token that can be used to cancel interactivity. Pass <see cref="System.Threading.CancellationToken.None"/> to wait indefinitely.</param>
        /// <exception cref="System.ArgumentException">Thrown when the Provided message does not contain any dropdowns</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage Message, Func<ComponentInteractionCreateEventArgs, bool> Predicate, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Select))
                throw new ArgumentException("Message does not contain any select components.");

            var result = await this
                ._componentEventWaiter
                .WaitForMatchAsync(new(Message, C => C.Interaction.Data.ComponentType is ComponentType.Select && Predicate(C), Token))
                .ConfigureAwait(false);

            return new(result is null, result);
        }

        /// <summary>
        /// Waits for a dropdown to be interacted with.
        /// </summary>
        /// <remarks>This is here for backwards-compatibility and will internally create a cancellation token.</remarks>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Id">The Id of the dropdown to wait on.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <exception cref="System.ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(DiscordMessage Message, string Id, TimeSpan? TimeoutOverride = null)
            => this.WaitForSelectAsync(Message, Id, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for a dropdown to be interacted with.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="Id">The Id of the dropdown to wait on.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        /// <exception cref="System.ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage Message, string Id, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Select))
                throw new ArgumentException("Message does not contain any select components.");

            if (Message.Components.SelectMany(C => C.Components).OfType<DiscordSelectComponent>().All(C => C.CustomId != Id))
                throw new ArgumentException($"Message does not contain select component with Id of '{Id}'.");

            var result = await this
                ._componentEventWaiter
                .WaitForMatchAsync(new(Message, (C) => C.Interaction.Data.ComponentType is ComponentType.Select && C.Id == Id, Token))
                .ConfigureAwait(false);

            return new(result is null, result);
        }

        /// <summary>
        /// Waits for a dropdown to be interacted with by a specific user.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="User">The user to wait on.</param>
        /// <param name="Id">The Id of the dropdown to wait on.</param>
        /// <param name="TimeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
        /// <exception cref="System.ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
        public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelect(DiscordMessage Message, DiscordUser User, string Id, TimeSpan? TimeoutOverride = null)
            => this.WaitForSelectAsync(Message, User, Id, this.GetCancellationToken(TimeoutOverride));

        /// <summary>
        /// Waits for a dropdown to be interacted with by a specific user.
        /// </summary>
        /// <param name="Message">The message to wait on.</param>
        /// <param name="User">The user to wait on.</param>
        /// <param name="Id">The Id of the dropdown to wait on.</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        /// <exception cref="System.ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
        public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage Message, DiscordUser User, string Id, CancellationToken Token)
        {
            if (Message.Author != this.Client.CurrentUser)
                throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

            if (!Message.Components.Any())
                throw new ArgumentException("Provided message does not contain any components.");

            if (!Message.Components.SelectMany(C => C.Components).Any(C => C.Type is ComponentType.Select))
                throw new ArgumentException("Message does not contain any select components.");

            if (Message.Components.SelectMany(C => C.Components).OfType<DiscordSelectComponent>().All(C => C.CustomId != Id))
                throw new ArgumentException($"Message does not contain select with Id of '{Id}'.");

            var result = await this
                ._componentEventWaiter
                .WaitForMatchAsync(new(Message, (C) => C.Id == Id && C.User == User, Token)).ConfigureAwait(false);

            return new(result is null, result);
        }


        /// <summary>
        /// Waits for a specific message.
        /// </summary>
        /// <param name="Predicate">Predicate to match.</param>
        /// <param name="Timeoutoverride">override timeout period.</param>
        public async Task<InteractivityResult<DiscordMessage>> WaitForMessageAsync(Func<DiscordMessage, bool> Predicate,
            TimeSpan? Timeoutoverride = null)
        {
            if (!Utilities.HasMessageIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No message intents are enabled.");

            var timeout = Timeoutoverride ?? this.Config.Timeout;
            var returns = await this._messageCreatedWaiter.WaitForMatchAsync(new MatchRequest<MessageCreateEventArgs>(X => Predicate(X.Message), timeout)).ConfigureAwait(false);

            return new InteractivityResult<DiscordMessage>(returns == null, returns?.Message);
        }

        /// <summary>
        /// Wait for a specific reaction.
        /// </summary>
        /// <param name="Predicate">Predicate to match.</param>
        /// <param name="Timeoutoverride">override timeout period.</param>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> Predicate,
            TimeSpan? Timeoutoverride = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            var timeout = Timeoutoverride ?? this.Config.Timeout;
            var returns = await this._messageReactionAddWaiter.WaitForMatchAsync(new MatchRequest<MessageReactionAddEventArgs>(Predicate, timeout)).ConfigureAwait(false);

            return new InteractivityResult<MessageReactionAddEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Wait for a specific reaction.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <param name="Message">Message reaction was added to.</param>
        /// <param name="User">User that made the reaction.</param>
        /// <param name="Timeoutoverride">override timeout period.</param>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(DiscordMessage Message, DiscordUser User,
            TimeSpan? Timeoutoverride = null)
            => await this.WaitForReactionAsync(X => X.User.Id == User.Id && X.Message.Id == Message.Id, Timeoutoverride).ConfigureAwait(false);

        /// <summary>
        /// Waits for a specific reaction.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <param name="Predicate">Predicate to match.</param>
        /// <param name="Message">Message reaction was added to.</param>
        /// <param name="User">User that made the reaction.</param>
        /// <param name="Timeoutoverride">override timeout period.</param>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> Predicate,
            DiscordMessage Message, DiscordUser User, TimeSpan? Timeoutoverride = null)
            => await this.WaitForReactionAsync(X => Predicate(X) && X.User.Id == User.Id && X.Message.Id == Message.Id, Timeoutoverride).ConfigureAwait(false);

        /// <summary>
        /// Waits for a specific reaction.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <param name="Predicate">predicate to match.</param>
        /// <param name="User">User that made the reaction.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> Predicate,
            DiscordUser User, TimeSpan? Timeoutoverride = null)
            => await this.WaitForReactionAsync(X => Predicate(X) && X.User.Id == User.Id, Timeoutoverride).ConfigureAwait(false);

        /// <summary>
        /// Waits for a user to start typing.
        /// </summary>
        /// <param name="User">User that starts typing.</param>
        /// <param name="Channel">Channel the user is typing in.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser User,
            DiscordChannel Channel, TimeSpan? Timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = Timeoutoverride ?? this.Config.Timeout;
            var returns = await this._typingStartWaiter.WaitForMatchAsync(
                new MatchRequest<TypingStartEventArgs>(X => X.User.Id == User.Id && X.Channel.Id == Channel.Id, timeout))
                .ConfigureAwait(false);

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Waits for a user to start typing.
        /// </summary>
        /// <param name="User">User that starts typing.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser User, TimeSpan? Timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = Timeoutoverride ?? this.Config.Timeout;
            var returns = await this._typingStartWaiter.WaitForMatchAsync(
                new MatchRequest<TypingStartEventArgs>(X => X.User.Id == User.Id, timeout))
                .ConfigureAwait(false);

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Waits for any user to start typing.
        /// </summary>
        /// <param name="Channel">Channel to type in.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForTypingAsync(DiscordChannel Channel, TimeSpan? Timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = Timeoutoverride ?? this.Config.Timeout;
            var returns = await this._typingStartWaiter.WaitForMatchAsync(
                new MatchRequest<TypingStartEventArgs>(X => X.Channel.Id == Channel.Id, timeout))
                .ConfigureAwait(false);

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Collects reactions on a specific message.
        /// </summary>
        /// <param name="M">Message to collect reactions on.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task<ReadOnlyCollection<Reaction>> CollectReactionsAsync(DiscordMessage M, TimeSpan? Timeoutoverride = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            var timeout = Timeoutoverride ?? this.Config.Timeout;
            var collection = await this._reactionCollector.CollectAsync(new ReactionCollectRequest(M, timeout)).ConfigureAwait(false);

            return collection;
        }

        /// <summary>
        /// Waits for specific event args to be received. Make sure the appropriate <see cref="DiscordIntents"/> are registered, if needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Predicate">The predicate.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task<InteractivityResult<T>> WaitForEventArgsAsync<T>(Func<T, bool> Predicate, TimeSpan? Timeoutoverride = null) where T : AsyncEventArgs
        {
            var timeout = Timeoutoverride ?? this.Config.Timeout;

            using var waiter = new EventWaiter<T>(this.Client);
            var res = await waiter.WaitForMatchAsync(new MatchRequest<T>(Predicate, timeout)).ConfigureAwait(false);
            return new InteractivityResult<T>(res == null, res);
        }

        /// <summary>
        /// Collects the event arguments.
        /// </summary>
        /// <param name="Predicate">The predicate.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task<ReadOnlyCollection<T>> CollectEventArgsAsync<T>(Func<T, bool> Predicate, TimeSpan? Timeoutoverride = null) where T : AsyncEventArgs
        {
            var timeout = Timeoutoverride ?? this.Config.Timeout;

            using var waiter = new EventWaiter<T>(this.Client);
            var res = await waiter.CollectMatchesAsync(new CollectRequest<T>(Predicate, timeout)).ConfigureAwait(false);
            return res;
        }

        /// <summary>
        /// Sends a paginated message with buttons.
        /// </summary>
        /// <param name="Channel">The channel to send it on.</param>
        /// <param name="User">User to give control.</param>
        /// <param name="Pages">The pages.</param>
        /// <param name="Buttons">Pagination buttons (pass null to use buttons defined in <see cref="InteractivityConfiguration"/>).</param>
        /// <param name="Behaviour">Pagination behaviour.</param>
        /// <param name="Deletion">Deletion behaviour</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        public async Task SendPaginatedMessageAsync(
            DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationButtons Buttons,
            PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default, CancellationToken Token = default)
        {
            var bhv = Behaviour ?? this.Config.PaginationBehaviour;
            var del = Deletion ?? this.Config.ButtonBehavior;
            var bts = Buttons ?? this.Config.PaginationButtons;

            bts = new(bts);
            if (bhv is PaginationBehaviour.Ignore)
            {
                bts.SkipLeft.Disable();
                bts.Left.Disable();
            }

            var builder = new DiscordMessageBuilder()
                .WithContent(Pages.First().Content)
                .WithEmbed(Pages.First().Embed)
                .AddComponents(bts.ButtonArray);

            var message = await builder.Send(Channel).ConfigureAwait(false);

            var req = new ButtonPaginationRequest(message, User, bhv, del, bts, Pages.ToArray(), Token == default ? this.GetCancellationToken() : Token);

            await this._compPaginator.DoPagination(req).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a paginated message with buttons.
        /// </summary>
        /// <param name="Channel">The channel to send it on.</param>
        /// <param name="User">User to give control.</param>
        /// <param name="Pages">The pages.</param>
        /// <param name="Buttons">Pagination buttons (pass null to use buttons defined in <see cref="InteractivityConfiguration"/>).</param>
        /// <param name="Behaviour">Pagination behaviour.</param>
        /// <param name="Deletion">Deletion behaviour</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public Task SendPaginatedMessage(
            DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationButtons Buttons, TimeSpan? Timeoutoverride,
            PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default)
            => this.SendPaginatedMessageAsync(Channel, User, Pages, Buttons, Behaviour, Deletion, this.GetCancellationToken(Timeoutoverride));

        /// <summary>
        /// Sends the paginated message.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        /// <param name="User">The user.</param>
        /// <param name="Pages">The pages.</param>
        /// <param name="Behaviour">The behaviour.</param>
        /// <param name="Deletion">The deletion.</param>
        /// <param name="Token">The token.</param>
        /// <returns>A Task.</returns>
        public Task SendPaginatedMessage(DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default, CancellationToken Token = default)
            => this.SendPaginatedMessageAsync(Channel, User, Pages, default, Behaviour, Deletion, Token);

        /// <summary>
        /// Sends the paginated message.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        /// <param name="User">The user.</param>
        /// <param name="Pages">The pages.</param>
        /// <param name="Timeoutoverride">The timeoutoverride.</param>
        /// <param name="Behaviour">The behaviour.</param>
        /// <param name="Deletion">The deletion.</param>
        /// <returns>A Task.</returns>
        public Task SendPaginatedMessage(DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, TimeSpan? Timeoutoverride, PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default)
            => this.SendPaginatedMessage(Channel, User, Pages, Timeoutoverride, Behaviour, Deletion);

        /// <summary>
        /// Sends a paginated message.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <param name="Channel">Channel to send paginated message in.</param>
        /// <param name="User">User to give control.</param>
        /// <param name="Pages">Pages.</param>
        /// <param name="Emojis">Pagination emojis.</param>
        /// <param name="Behaviour">Pagination behaviour (when hitting max and min indices).</param>
        /// <param name="Deletion">Deletion behaviour.</param>
        /// <param name="Timeoutoverride">Override timeout period.</param>
        public async Task SendPaginatedMessageAsync(DiscordChannel Channel, DiscordUser User, IEnumerable<Page> Pages, PaginationEmojis Emojis,
            PaginationBehaviour? Behaviour = default, PaginationDeletion? Deletion = default, TimeSpan? Timeoutoverride = null)
        {
            var builder = new DiscordMessageBuilder()
                .WithContent(Pages.First().Content)
                .WithEmbed(Pages.First().Embed);
            var m = await builder.Send(Channel).ConfigureAwait(false);

            var timeout = Timeoutoverride ?? this.Config.Timeout;

            var bhv = Behaviour ?? this.Config.PaginationBehaviour;
            var del = Deletion ?? this.Config.PaginationDeletion;
            var ems = Emojis ?? this.Config.PaginationEmojis;

            var prequest = new PaginationRequest(m, User, bhv, del, ems, timeout, Pages.ToArray());

            await this._paginator.DoPagination(prequest).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a paginated message in response to an interaction.
        /// <para>
        /// <b>Pass the interaction directly. Interactivity will ACK it.</b>
        /// </para>
        /// </summary>
        /// <param name="Interaction">The interaction to create a response to.</param>
        /// <param name="Ephemeral">Whether the response should be ephemeral.</param>
        /// <param name="User">The user to listen for button presses from.</param>
        /// <param name="Pages">The pages to paginate.</param>
        /// <param name="Buttons">Optional: custom buttons</param>
        /// <param name="Behaviour">Pagination behaviour.</param>
        /// <param name="Deletion">Deletion behaviour</param>
        /// <param name="Token">A custom cancellation token that can be cancelled at any point.</param>
        public async Task SendPaginatedResponseAsync(DiscordInteraction Interaction, bool Ephemeral, DiscordUser User, IEnumerable<Page> Pages, PaginationButtons Buttons = null, PaginationBehaviour? Behaviour = default, ButtonPaginationBehavior? Deletion = default, CancellationToken Token = default)
        {
            var bhv = Behaviour ?? this.Config.PaginationBehaviour;
            var del = Deletion ?? this.Config.ButtonBehavior;
            var bts = Buttons ?? this.Config.PaginationButtons;

            bts = new(bts);
            if (bhv is PaginationBehaviour.Ignore)
            {
                bts.SkipLeft.Disable();
                bts.Left.Disable();
            }

            var builder = new DiscordInteractionResponseBuilder()
                .WithContent(Pages.First().Content)
                .AddEmbed(Pages.First().Embed)
                .AsEphemeral(Ephemeral)
                .AddComponents(bts.ButtonArray);

            await Interaction.CreateResponse(InteractionResponseType.ChannelMessageWithSource, builder).ConfigureAwait(false);
            var message = await Interaction.GetOriginalResponse().ConfigureAwait(false);

            var req = new InteractionPaginationRequest(Interaction, message, User, bhv, del, bts, Pages, Token);

            await this._compPaginator.DoPagination(req).ConfigureAwait(false);
        }


        /// <summary>
        /// Waits for a custom pagination request to finish.
        /// This does NOT handle removing emojis after finishing for you.
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public async Task WaitForCustomPaginationAsync(IPaginationRequest Request) => await this._paginator.DoPagination(Request).ConfigureAwait(false);

        /// <summary>
        /// Waits for custom button-based pagination request to finish.
        /// <br/>
        /// This does <i><b>not</b></i> invoke <see cref="IPaginationRequest.DoCleanup"/>.
        /// </summary>
        /// <param name="Request">The request to wait for.</param>
        public async Task WaitForCustomComponentPaginationAsync(IPaginationRequest Request) => await this._compPaginator.DoPagination(Request).ConfigureAwait(false);

        /// <summary>
        /// Generates pages from a string, and puts them in message content.
        /// </summary>
        /// <param name="Input">Input string.</param>
        /// <param name="Splittype">How to split input string.</param>
        /// <returns></returns>
        public IEnumerable<Page> GeneratePagesInContent(string Input, SplitType Splittype = SplitType.Character)
        {
            if (string.IsNullOrEmpty(Input))
                throw new ArgumentException("You must provide a string that is not null or empty!");

            var result = new List<Page>();
            List<string> split;

            switch (Splittype)
            {
                default:
                case SplitType.Character:
                    split = this.SplitString(Input, 500).ToList();
                    break;
                case SplitType.Line:
                    var subsplit = Input.Split('\n');

                    split = new List<string>();
                    var s = "";

                    for (var i = 0; i < subsplit.Length; i++)
                    {
                        s += subsplit[i];
                        if (i >= 15 && i % 15 == 0)
                        {
                            split.Add(s);
                            s = "";
                        }
                    }
                    if (split.All(X => X != s))
                        split.Add(s);
                    break;
            }

            var page = 1;
            foreach (var s in split)
            {
                result.Add(new Page($"Page {page}:\n{s}"));
                page++;
            }

            return result;
        }

        /// <summary>
        /// Generates pages from a string, and puts them in message embeds.
        /// </summary>
        /// <param name="Input">Input string.</param>
        /// <param name="Splittype">How to split input string.</param>
        /// <param name="Embedbase">Base embed for output embeds.</param>
        /// <returns></returns>
        public IEnumerable<Page> GeneratePagesInEmbed(string Input, SplitType Splittype = SplitType.Character, DiscordEmbedBuilder Embedbase = null)
        {
            if (string.IsNullOrEmpty(Input))
                throw new ArgumentException("You must provide a string that is not null or empty!");

            var embed = Embedbase ?? new DiscordEmbedBuilder();

            var result = new List<Page>();
            List<string> split;

            switch (Splittype)
            {
                default:
                case SplitType.Character:
                    split = this.SplitString(Input, 500).ToList();
                    break;
                case SplitType.Line:
                    var subsplit = Input.Split('\n');

                    split = new List<string>();
                    var s = "";

                    for (var i = 0; i < subsplit.Length; i++)
                    {
                        s += $"{subsplit[i]}\n";
                        if (i % 15 == 0 && i != 0)
                        {
                            split.Add(s);
                            s = "";
                        }
                    }
                    if (!split.Any(X => X == s))
                        split.Add(s);
                    break;
            }

            var page = 1;
            foreach (var s in split)
            {
                result.Add(new Page("", new DiscordEmbedBuilder(embed).WithDescription(s).WithFooter($"Page {page}/{split.Count}")));
                page++;
            }

            return result;
        }

        /// <summary>
        /// Splits the string.
        /// </summary>
        /// <param name="Str">The string.</param>
        /// <param name="ChunkSize">The chunk size.</param>
        private List<string> SplitString(string Str, int ChunkSize)
        {
            var res = new List<string>();
            var len = Str.Length;
            var i = 0;

            while (i < len)
            {
                var size = Math.Min(len - i, ChunkSize);
                res.Add(Str.Substring(i, size));
                i += size;
            }

            return res;
        }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <param name="Timeout">The timeout.</param>
        private CancellationToken GetCancellationToken(TimeSpan? Timeout = null) => new CancellationTokenSource(Timeout ?? this.Config.Timeout).Token;

        /// <summary>
        /// Handles an invalid interaction.
        /// </summary>
        /// <param name="Interaction">The interaction.</param>
        private async Task HandleInvalidInteraction(DiscordInteraction Interaction)
        {
            var at = this.Config.ResponseBehavior switch
            {
                InteractionResponseBehavior.Ack => Interaction.CreateResponse(InteractionResponseType.DeferredMessageUpdate),
                InteractionResponseBehavior.Respond => Interaction.CreateResponse(InteractionResponseType.ChannelMessageWithSource, new() { Content = this.Config.ResponseMessage, IsEphemeral = true}),
                InteractionResponseBehavior.Ignore => Task.CompletedTask,
                _ => throw new ArgumentException("Unknown enum value.")
            };

            await at;
        }
    }
}
