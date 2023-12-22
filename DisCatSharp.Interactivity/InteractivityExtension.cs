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

namespace DisCatSharp.Interactivity;

/// <summary>
/// Extension class for DisCatSharp.Interactivity
/// </summary>
public class InteractivityExtension : BaseExtension
{
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
	private ComponentPaginator _compPaginator;

	/// <summary>
	/// Initializes a new instance of the <see cref="InteractivityExtension"/> class.
	/// </summary>
	/// <param name="cfg">The configuration.</param>
	internal InteractivityExtension(InteractivityConfiguration cfg)
	{
		this.Config = new(cfg);
	}

	/// <summary>
	/// Setups the Interactivity Extension.
	/// </summary>
	/// <param name="client">Discord client.</param>
	protected internal override void Setup(DiscordClient client)
	{
		this.Client = client;
		this._messageCreatedWaiter = new(this.Client);
		this._messageReactionAddWaiter = new(this.Client);
		this._componentInteractionWaiter = new(this.Client);
		this._modalInteractionWaiter = new(this.Client);
		this._typingStartWaiter = new(this.Client);
		this._poller = new(this.Client);
		this._reactionCollector = new(this.Client);
		this._paginator = new(this.Client);
		this._compPaginator = new(this.Client, this.Config);
		this._componentEventWaiter = new(this.Client, this.Config);
		this._modalEventWaiter = new(this.Client, this.Config);
	}

	/// <summary>
	/// Makes a poll and returns poll results.
	/// </summary>
	/// <param name="m">Message to create poll on.</param>
	/// <param name="emojis">Emojis to use for this poll.</param>
	/// <param name="behaviour">What to do when the poll ends.</param>
	/// <param name="timeout">Override timeout period.</param>
	/// <returns></returns>
	public async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(DiscordMessage m, IEnumerable<DiscordEmoji> emojis, PollBehaviour? behaviour = default, TimeSpan? timeout = null)
	{
		if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
			throw new InvalidOperationException("No reaction intents are enabled.");

		if (!emojis.Any())
			throw new ArgumentException("You need to provide at least one emoji for a poll!");

		foreach (var em in emojis)
			await m.CreateReactionAsync(em).ConfigureAwait(false);

		var res = await this._poller.DoPollAsync(new(m, timeout ?? this.Config.Timeout, emojis)).ConfigureAwait(false);

		var pollBehaviour = behaviour ?? this.Config.PollBehaviour;
		var thisMember = await m.Channel.Guild.GetMemberAsync(this.Client.CurrentUser.Id).ConfigureAwait(false);

		if (pollBehaviour == PollBehaviour.DeleteEmojis && m.Channel.PermissionsFor(thisMember).HasPermission(Permissions.ManageMessages))
			await m.DeleteAllReactionsAsync().ConfigureAwait(false);

		return new(res.ToList());
	}

	/// <summary>
	/// Waits for any button in the specified collection to be pressed.
	/// </summary>
	/// <param name="message">The message to wait on.</param>
	/// <param name="buttons">A collection of buttons to listen for.</param>
	/// <param name="timeoutOverride">Override the timeout period in <see cref="InteractivityConfiguration"/>.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, IEnumerable<DiscordButtonComponent> buttons, TimeSpan? timeoutOverride = null)
		=> this.WaitForButtonAsync(message, buttons, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for any button in the specified collection to be pressed.
	/// </summary>
	/// <param name="message">The message to wait on.</param>
	/// <param name="buttons">A collection of buttons to listen for.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, IEnumerable<DiscordButtonComponent> buttons, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (!buttons.Any())
			throw new ArgumentException("You must specify at least one button to listen for.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is ComponentType.Button))
			throw new ArgumentException("Provided Message does not contain any button components.");

		var res = await this._componentEventWaiter
			.WaitForMatchAsync(new(message,
				c =>
					c.Interaction.Data.ComponentType == ComponentType.Button &&
					buttons.Any(b => b.CustomId == c.Id), token)).ConfigureAwait(false);

		return new(res is null, res);
	}

	/// <summary>
	/// Waits for a user modal submit.
	/// </summary>
	/// <param name="customId">The custom id of the modal to wait for.</param>
	/// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of the modal.</returns>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForModalAsync(string customId, TimeSpan? timeoutOverride = null)
		=> this.WaitForModalAsync(customId, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for a user modal submit.
	/// </summary>
	/// <param name="customId">The custom id of the modal to wait for.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of the modal.</returns>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForModalAsync(string customId, CancellationToken token)
	{
		var result =
			await this
				._modalEventWaiter
				.WaitForModalMatchAsync(new(customId, c => c.Interaction.Type == InteractionType.ModalSubmit, token))
				.ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for any button on the specified message to be pressed.
	/// </summary>
	/// <param name="message">The message to wait for the button on.</param>
	/// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, TimeSpan? timeoutOverride = null)
		=> this.WaitForButtonAsync(message, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for any button on the specified message to be pressed.
	/// </summary>
	/// <param name="message">The message to wait for the button on.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is ComponentType.Button))
			throw new ArgumentException("Message does not contain any button components.");

		var ids = message.Components.SelectMany(m => m.Components).Select(c => c.CustomId);

		var result =
			await this
				._componentEventWaiter
				.WaitForMatchAsync(new(message, c => c.Interaction.Data.ComponentType == ComponentType.Button && ids.Contains(c.Id), token))
				.ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for any button on the specified message to be pressed by the specified user.
	/// </summary>
	/// <param name="message">The message to wait for the button on.</param>
	/// <param name="user">The user to wait for the button press from.</param>
	/// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, DiscordUser user, TimeSpan? timeoutOverride = null)
		=> this.WaitForButtonAsync(message, user, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for any button on the specified message to be pressed by the specified user.
	/// </summary>
	/// <param name="message">The message to wait for the button on.</param>
	/// <param name="user">The user to wait for the button press from.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of button that was pressed, if any.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, DiscordUser user, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is ComponentType.Button))
			throw new ArgumentException("Message does not contain any button components.");

		var result = await this
			._componentEventWaiter
			.WaitForMatchAsync(new(message, (c) => c.Interaction.Data.ComponentType is ComponentType.Button && c.User == user, token))
			.ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for a button with the specified Id to be pressed.
	/// </summary>
	/// <param name="message">The message to wait for the button on.</param>
	/// <param name="id">The Id of the button to wait for.</param>
	/// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of the operation.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, string id, TimeSpan? timeoutOverride = null)
		=> this.WaitForButtonAsync(message, id, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for a button with the specified Id to be pressed.
	/// </summary>
	/// <param name="message">The message to wait for the button on.</param>
	/// <param name="id">The Id of the button to wait for.</param>
	/// <param name="token">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <returns>A <see cref="InteractivityResult{T}"/> with the result of the operation.</returns>
	/// <exception cref="InvalidOperationException">Thrown when attempting to wait for a message that is not authored by the current user.</exception>
	/// <exception cref="ArgumentException">Thrown when the message does not contain a button with the specified Id, or any buttons at all.</exception>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, string id, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is ComponentType.Button))
			throw new ArgumentException("Message does not contain any button components.");

		if (!message.Components.SelectMany(c => c.Components).OfType<DiscordButtonComponent>().Any(c => c.CustomId == id))
			throw new ArgumentException($"Message does not contain button with Id of '{id}'.");

		var result = await this
			._componentEventWaiter
			.WaitForMatchAsync(new(message, (c) => c.Interaction.Data.ComponentType is ComponentType.Button && c.Id == id, token))
			.ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for any button to be interacted with.
	/// </summary>
	/// <param name="message">The message to wait on.</param>
	/// <param name="predicate">The predicate to filter interactions by.</param>
	/// <param name="timeoutOverride">Override the timeout specified in <see cref="InteractivityConfiguration"/></param>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, TimeSpan? timeoutOverride = null)
		=> this.WaitForButtonAsync(message, predicate, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for any button to be interacted with.
	/// </summary>
	/// <param name="message">The message to wait on.</param>
	/// <param name="predicate">The predicate to filter interactions by.</param>
	/// <param name="token">A token to cancel interactivity with at any time. Pass <see cref="CancellationToken.None"/> to wait indefinitely.</param>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type is ComponentType.Button))
			throw new ArgumentException("Message does not contain any button components.");

		var result = await this
			._componentEventWaiter
			.WaitForMatchAsync(new(message, c => c.Interaction.Data.ComponentType is ComponentType.Button && predicate(c), token))
			.ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for any dropdown to be interacted with.
	/// </summary>
	/// <param name="message">The message to wait for.</param>
	/// <param name="predicate">A filter predicate.</param>
	/// <param name="selectType">The <see cref="ComponentType">type</see> of the select menu.</param>
	/// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <exception cref="ArgumentException">Thrown when the Provided message does not contain any dropdowns</exception>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, ComponentType selectType, TimeSpan? timeoutOverride = null)
		=> this.WaitForSelectAsync(message, predicate, selectType, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for any dropdown to be interacted with.
	/// </summary>
	/// <param name="message">The message to wait for.</param>
	/// <param name="predicate">A filter predicate.</param>
	/// <param name="selectType">The <see cref="ComponentType">type</see> of the select menu.</param>
	/// <param name="token">A token that can be used to cancel interactivity. Pass <see cref="CancellationToken.None"/> to wait indefinitely.</param>
	/// <exception cref="ArgumentException">Thrown when the Provided message does not contain any dropdowns</exception>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, ComponentType selectType, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type == selectType))
			throw new ArgumentException("Message does not contain any select components.");

		var result = await this
			._componentEventWaiter
			.WaitForMatchAsync(new(message, c => c.Interaction.Data.ComponentType == selectType && predicate(c), token))
			.ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for a dropdown to be interacted with.
	/// </summary>
	/// <remarks>This is here for backwards-compatibility and will internally create a cancellation token.</remarks>
	/// <param name="message">The message to wait on.</param>
	/// <param name="id">The Id of the dropdown to wait on.</param>
	/// <param name="selectType">The <see cref="ComponentType">type</see> of the select menu.</param>
	/// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, string id, ComponentType selectType, TimeSpan? timeoutOverride = null)
		=> this.WaitForSelectAsync(message, id, selectType, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for a dropdown to be interacted with.
	/// </summary>
	/// <param name="message">The message to wait on.</param>
	/// <param name="id">The Id of the dropdown to wait on.</param>
	/// <param name="selectType">The <see cref="ComponentType">type</see> of the select menu.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	/// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, string id, ComponentType selectType, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type == selectType))
			throw new ArgumentException("Message does not contain any select components.");

		if (message.Components.SelectMany(c => c.Components).OfType<DiscordBaseSelectComponent>().All(c => c.CustomId != id))
			throw new ArgumentException($"Message does not contain select component with Id of '{id}'.");

		var result = await this
			._componentEventWaiter
			.WaitForMatchAsync(new(message, (c) => c.Interaction.Data.ComponentType == selectType && c.Id == id, token))
			.ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for a dropdown to be interacted with by a specific user.
	/// </summary>
	/// <param name="message">The message to wait on.</param>
	/// <param name="user">The user to wait on.</param>
	/// <param name="id">The Id of the dropdown to wait on.</param>
	/// <param name="selectType">The <see cref="ComponentType">type</see> of the select menu.</param>
	/// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
	/// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
	public Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, DiscordUser user, string id, ComponentType selectType, TimeSpan? timeoutOverride = null)
		=> this.WaitForSelectAsync(message, user, id, selectType, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Waits for a dropdown to be interacted with by a specific user.
	/// </summary>
	/// <param name="message">The message to wait on.</param>
	/// <param name="user">The user to wait on.</param>
	/// <param name="id">The Id of the dropdown to wait on.</param>
	/// <param name="selectType">The <see cref="ComponentType">type</see> of the select menu.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	/// <exception cref="ArgumentException">Thrown when the message does not have any dropdowns or any dropdown with the specified Id.</exception>
	public async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(DiscordMessage message, DiscordUser user, string id, ComponentType selectType, CancellationToken token)
	{
		if (message.Author != this.Client.CurrentUser)
			throw new InvalidOperationException("Interaction events are only sent to the application that created them.");

		if (message.Components.Count == 0)
			throw new ArgumentException("Provided message does not contain any components.");

		if (!message.Components.SelectMany(c => c.Components).Any(c => c.Type == selectType))
			throw new ArgumentException("Message does not contain any select components.");

		if (message.Components.SelectMany(c => c.Components).OfType<DiscordBaseSelectComponent>().All(c => c.CustomId != id))
			throw new ArgumentException($"Message does not contain select with Id of '{id}'.");

		var result = await this
			._componentEventWaiter
			.WaitForMatchAsync(new(message, (c) => c.Id == id && c.User == user, token)).ConfigureAwait(false);

		return new(result is null, result);
	}

	/// <summary>
	/// Waits for a specific message.
	/// </summary>
	/// <param name="predicate">Predicate to match.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<DiscordMessage>> WaitForMessageAsync(
		Func<DiscordMessage, bool> predicate,
		TimeSpan? timeoutOverride = null
	)
	{
		if (!Utilities.HasMessageIntents(this.Client.Configuration.Intents))
			throw new InvalidOperationException("No message intents are enabled.");

		var timeout = timeoutOverride ?? this.Config.Timeout;
		var returns = await this._messageCreatedWaiter.WaitForMatchAsync(new(x => predicate(x.Message), timeout)).ConfigureAwait(false);

		return new(returns == null, returns?.Message);
	}

	/// <summary>
	/// Wait for a specific reaction.
	/// </summary>
	/// <param name="predicate">Predicate to match.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(
		Func<MessageReactionAddEventArgs, bool> predicate,
		TimeSpan? timeoutOverride = null
	)
	{
		if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
			throw new InvalidOperationException("No reaction intents are enabled.");

		var timeout = timeoutOverride ?? this.Config.Timeout;
		var returns = await this._messageReactionAddWaiter.WaitForMatchAsync(new(predicate, timeout)).ConfigureAwait(false);

		return new(returns == null, returns);
	}

	/// <summary>
	/// Wait for a specific reaction.
	/// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
	/// </summary>
	/// <param name="message">Message reaction was added to.</param>
	/// <param name="user">User that made the reaction.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(
		DiscordMessage message,
		DiscordUser user,
		TimeSpan? timeoutOverride = null
	)
		=> await this.WaitForReactionAsync(x => x.User.Id == user.Id && x.Message.Id == message.Id, timeoutOverride).ConfigureAwait(false);

	/// <summary>
	/// Waits for a specific reaction.
	/// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
	/// </summary>
	/// <param name="predicate">Predicate to match.</param>
	/// <param name="message">Message reaction was added to.</param>
	/// <param name="user">User that made the reaction.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(
		Func<MessageReactionAddEventArgs, bool> predicate,
		DiscordMessage message,
		DiscordUser user,
		TimeSpan? timeoutOverride = null
	)
		=> await this.WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id && x.Message.Id == message.Id, timeoutOverride).ConfigureAwait(false);

	/// <summary>
	/// Waits for a specific reaction.
	/// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
	/// </summary>
	/// <param name="predicate">predicate to match.</param>
	/// <param name="user">User that made the reaction.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(
		Func<MessageReactionAddEventArgs, bool> predicate,
		DiscordUser user,
		TimeSpan? timeoutOverride = null
	)
		=> await this.WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id, timeoutOverride).ConfigureAwait(false);

	/// <summary>
	/// Waits for a user to start typing.
	/// </summary>
	/// <param name="user">User that starts typing.</param>
	/// <param name="channel">Channel the user is typing in.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(
		DiscordUser user,
		DiscordChannel channel,
		TimeSpan? timeoutOverride = null
	)
	{
		if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
			throw new InvalidOperationException("No typing intents are enabled.");

		var timeout = timeoutOverride ?? this.Config.Timeout;
		var returns = await this._typingStartWaiter.WaitForMatchAsync(
				new(x => x.User.Id == user.Id && x.Channel.Id == channel.Id, timeout))
			.ConfigureAwait(false);

		return new(returns == null, returns);
	}

	/// <summary>
	/// Waits for a user to start typing.
	/// </summary>
	/// <param name="user">User that starts typing.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser user, TimeSpan? timeoutOverride = null)
	{
		if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
			throw new InvalidOperationException("No typing intents are enabled.");

		var timeout = timeoutOverride ?? this.Config.Timeout;
		var returns = await this._typingStartWaiter.WaitForMatchAsync(
				new(x => x.User.Id == user.Id, timeout))
			.ConfigureAwait(false);

		return new(returns == null, returns);
	}

	/// <summary>
	/// Waits for any user to start typing.
	/// </summary>
	/// <param name="channel">Channel to type in.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<TypingStartEventArgs>> WaitForTypingAsync(DiscordChannel channel, TimeSpan? timeoutOverride = null)
	{
		if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
			throw new InvalidOperationException("No typing intents are enabled.");

		var timeout = timeoutOverride ?? this.Config.Timeout;
		var returns = await this._typingStartWaiter.WaitForMatchAsync(
				new(x => x.Channel.Id == channel.Id, timeout))
			.ConfigureAwait(false);

		return new(returns == null, returns);
	}

	/// <summary>
	/// Collects reactions on a specific message.
	/// </summary>
	/// <param name="m">Message to collect reactions on.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<ReadOnlyCollection<Reaction>> CollectReactionsAsync(DiscordMessage m, TimeSpan? timeoutOverride = null)
	{
		if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
			throw new InvalidOperationException("No reaction intents are enabled.");

		var timeout = timeoutOverride ?? this.Config.Timeout;
		var collection = await this._reactionCollector.CollectAsync(new(m, timeout)).ConfigureAwait(false);

		return collection;
	}

	/// <summary>
	/// Waits for specific event args to be received. Make sure the appropriate <see cref="DiscordIntents"/> are registered, if needed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="predicate">The predicate.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<InteractivityResult<T>> WaitForEventArgsAsync<T>(Func<T, bool> predicate, TimeSpan? timeoutOverride = null) where T : AsyncEventArgs
	{
		var timeout = timeoutOverride ?? this.Config.Timeout;

		using var waiter = new EventWaiter<T>(this.Client);
		var res = await waiter.WaitForMatchAsync(new(predicate, timeout)).ConfigureAwait(false);
		return new(res == null, res);
	}

	/// <summary>
	/// Collects the event arguments.
	/// </summary>
	/// <param name="predicate">The predicate.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task<ReadOnlyCollection<T>> CollectEventArgsAsync<T>(Func<T, bool> predicate, TimeSpan? timeoutOverride = null) where T : AsyncEventArgs
	{
		var timeout = timeoutOverride ?? this.Config.Timeout;

		using var waiter = new EventWaiter<T>(this.Client);
		var res = await waiter.CollectMatchesAsync(new(predicate, timeout)).ConfigureAwait(false);
		return res;
	}

	/// <summary>
	/// Sends a paginated message with buttons.
	/// </summary>
	/// <param name="channel">The channel to send it on.</param>
	/// <param name="user">User to give control.</param>
	/// <param name="pages">The pages.</param>
	/// <param name="buttons">Pagination buttons (pass null to use buttons defined in <see cref="InteractivityConfiguration"/>).</param>
	/// <param name="behaviour">Pagination behaviour.</param>
	/// <param name="deletion">Deletion behaviour.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	public async Task SendPaginatedMessageAsync(
		DiscordChannel channel,
		DiscordUser user,
		IEnumerable<Page> pages,
		PaginationButtons buttons,
		PaginationBehaviour? behaviour = default,
		ButtonPaginationBehavior? deletion = default,
		CancellationToken token = default
	)
	{
		var bhv = behaviour ?? this.Config.PaginationBehaviour;
		var del = deletion ?? this.Config.ButtonBehavior;
		var bts = buttons ?? this.Config.PaginationButtons;

		bts = new(bts);
		if (bhv is PaginationBehaviour.Ignore)
		{
			bts.SkipLeft.Disable();
			bts.Left.Disable();
		}

		var builder = new DiscordMessageBuilder()
			.WithContent(pages.First().Content)
			.WithEmbed(pages.First().Embed)
			.AddComponents(bts.ButtonArray);

		var message = await builder.SendAsync(channel).ConfigureAwait(false);

		var req = new ButtonPaginationRequest(message, user, bhv, del, bts, pages, token == default ? this.GetCancellationToken() : token);

		await this._compPaginator.DoPaginationAsync(req).ConfigureAwait(false);
	}

	/// <summary>
	/// Sends a paginated message with buttons.
	/// </summary>
	/// <param name="channel">The channel to send it on.</param>
	/// <param name="user">User to give control.</param>
	/// <param name="pages">The pages.</param>
	/// <param name="buttons">Pagination buttons (pass null to use buttons defined in <see cref="InteractivityConfiguration"/>).</param>
	/// <param name="behaviour">Pagination behaviour.</param>
	/// <param name="deletion">Deletion behaviour.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public Task SendPaginatedMessageAsync(
		DiscordChannel channel,
		DiscordUser user,
		IEnumerable<Page> pages,
		PaginationButtons buttons,
		TimeSpan? timeoutOverride,
		PaginationBehaviour? behaviour = default,
		ButtonPaginationBehavior? deletion = default
	)
		=> this.SendPaginatedMessageAsync(channel, user, pages, buttons, behaviour, deletion, this.GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Sends the paginated message.
	/// </summary>
	/// <param name="channel">The channel.</param>
	/// <param name="user">The user.</param>
	/// <param name="pages">The pages.</param>
	/// <param name="behaviour">The behaviour.</param>
	/// <param name="deletion">The deletion.</param>
	/// <param name="token">The token.</param>
	/// <returns>A Task.</returns>
	public Task SendPaginatedMessageAsync(DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
		=> this.SendPaginatedMessageAsync(channel, user, pages, default, behaviour, deletion, token);

	/// <summary>
	/// Sends the paginated message.
	/// </summary>
	/// <param name="channel">The channel.</param>
	/// <param name="user">The user.</param>
	/// <param name="pages">The pages.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	/// <param name="behaviour">The behaviour.</param>
	/// <param name="deletion">The deletion.</param>
	/// <returns>A Task.</returns>
	public Task SendPaginatedMessageAsync(DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, TimeSpan? timeoutOverride, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default)
		=> this.SendPaginatedMessageAsync(channel, user, pages, timeoutOverride, behaviour, deletion);

	/// <summary>
	/// Sends a paginated message.
	/// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
	/// </summary>
	/// <param name="channel">Channel to send paginated message in.</param>
	/// <param name="user">User to give control.</param>
	/// <param name="pages">Pages.</param>
	/// <param name="emojis">Pagination emojis.</param>
	/// <param name="behaviour">Pagination behaviour (when hitting max and min indices).</param>
	/// <param name="deletion">Deletion behaviour.</param>
	/// <param name="timeoutOverride">Override timeout period.</param>
	public async Task SendPaginatedMessageAsync(
		DiscordChannel channel,
		DiscordUser user,
		IEnumerable<Page> pages,
		PaginationEmojis emojis,
		PaginationBehaviour? behaviour = default,
		PaginationDeletion? deletion = default,
		TimeSpan? timeoutOverride = null
	)
	{
		var builder = new DiscordMessageBuilder()
			.WithContent(pages.First().Content)
			.WithEmbed(pages.First().Embed);
		var m = await builder.SendAsync(channel).ConfigureAwait(false);

		var timeout = timeoutOverride ?? this.Config.Timeout;

		var bhv = behaviour ?? this.Config.PaginationBehaviour;
		var del = deletion ?? this.Config.PaginationDeletion;
		var ems = emojis ?? this.Config.PaginationEmojis;

		var pRequest = new PaginationRequest(m, user, bhv, del, ems, timeout, pages);

		await this._paginator.DoPaginationAsync(pRequest).ConfigureAwait(false);
	}

	/// <summary>
	/// Sends a paginated message in response to an interaction.
	/// <para>
	/// <b>Pass the interaction directly. Interactivity will ACK it.</b>
	/// </para>
	/// </summary>
	/// <param name="interaction">The interaction to create a response to.</param>
	/// <param name="deferred">Whether the interaction was deferred.</param>
	/// <param name="ephemeral">Whether the response should be ephemeral.</param>
	/// <param name="user">The user to listen for button presses from.</param>
	/// <param name="pages">The pages to paginate.</param>
	/// <param name="buttons">Optional: custom buttons.</param>
	/// <param name="behaviour">Pagination behaviour.</param>
	/// <param name="deletion">Deletion behaviour.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	public async Task SendPaginatedResponseAsync(DiscordInteraction interaction, bool deferred, bool ephemeral, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons = null, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
	{
		var bhv = behaviour ?? this.Config.PaginationBehaviour;
		var del = deletion ?? this.Config.ButtonBehavior;
		var bts = buttons ?? this.Config.PaginationButtons;

		bts = new(bts);
		if (bhv is PaginationBehaviour.Ignore)
		{
			bts.SkipLeft.Disable();
			bts.Left.Disable();
		}

		if (pages.Count() == 1)
		{
			bts.SkipRight.Disable();
			bts.Left.Disable();
			bts.Stop.Disable();
			bts.Right.Disable();
			bts.SkipRight.Disable();
		}

		DiscordMessage message;

		if (deferred)
		{
			var builder = new DiscordWebhookBuilder()
				.WithContent(pages.First().Content)
				.AddEmbed(pages.First().Embed)
				.AddComponents(bts.ButtonArray);
			message = await interaction.EditOriginalResponseAsync(builder).ConfigureAwait(false);
		}
		else
		{
			var builder = new DiscordInteractionResponseBuilder()
				.WithContent(pages.First().Content)
				.AddEmbed(pages.First().Embed)
				.AddComponents(bts.ButtonArray);
			if (ephemeral)
				builder = builder.AsEphemeral();
			await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder).ConfigureAwait(false);
			message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
		}

		var req = new InteractionPaginationRequest(interaction, message, user, bhv, del, bts, pages, token);

		await this._compPaginator.DoPaginationAsync(req).ConfigureAwait(false);
	}

	/// <summary>
	/// Waits for a custom pagination request to finish.
	/// This does NOT handle removing emojis after finishing for you.
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	public async Task WaitForCustomPaginationAsync(IPaginationRequest request) => await this._paginator.DoPaginationAsync(request).ConfigureAwait(false);

	/// <summary>
	/// Waits for custom button-based pagination request to finish.
	/// <br/>
	/// This does <i><b>not</b></i> invoke <see cref="DisCatSharp.Interactivity.EventHandling.IPaginationRequest.DoCleanupAsync"/>.
	/// </summary>
	/// <param name="request">The request to wait for.</param>
	public async Task WaitForCustomComponentPaginationAsync(IPaginationRequest request) => await this._compPaginator.DoPaginationAsync(request).ConfigureAwait(false);

	/// <summary>
	/// Generates pages from a string, and puts them in message content.
	/// </summary>
	/// <param name="input">Input string.</param>
	/// <param name="splitType">How to split input string.</param>
	/// <returns></returns>
	public IEnumerable<Page> GeneratePagesInContent(string input, SplitType splitType = SplitType.Character)
	{
		if (string.IsNullOrEmpty(input))
			throw new ArgumentException("You must provide a string that is not null or empty!");

		var result = new List<Page>();
		List<string> split;

		switch (splitType)
		{
			default:
			case SplitType.Character:
				split = [.. this.SplitString(input, 500)];
				break;
			case SplitType.Line:
				var subsplit = input.Split('\n');

				split = [];
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

				if (split.All(x => x != s))
					split.Add(s);
				break;
		}

		var page = 1;
		foreach (var s in split)
		{
			result.Add(new($"Page {page}:\n{s}"));
			page++;
		}

		return result;
	}

	/// <summary>
	/// Generates pages from a string, and puts them in message embeds.
	/// </summary>
	/// <param name="input">Input string.</param>
	/// <param name="splitType">How to split input string.</param>
	/// <param name="embedBase">Base embed for output embeds.</param>
	/// <returns></returns>
	public IEnumerable<Page> GeneratePagesInEmbed(string input, SplitType splitType = SplitType.Character, DiscordEmbedBuilder embedBase = null)
	{
		if (string.IsNullOrEmpty(input))
			throw new ArgumentException("You must provide a string that is not null or empty!");

		var embed = embedBase ?? new DiscordEmbedBuilder();

		var result = new List<Page>();
		List<string> split;

		switch (splitType)
		{
			default:
			case SplitType.Character:
				split = [.. this.SplitString(input, 500)];
				break;
			case SplitType.Line:
				var subsplit = input.Split('\n');

				split = [];
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

				if (!split.Any(x => x == s))
					split.Add(s);
				break;
		}

		var page = 1;
		foreach (var s in split)
		{
			result.Add(new("", new DiscordEmbedBuilder(embed).WithDescription(s).WithFooter($"Page {page}/{split.Count}")));
			page++;
		}

		return result;
	}

	/// <summary>
	/// Splits the string.
	/// </summary>
	/// <param name="str">The string.</param>
	/// <param name="chunkSize">The chunk size.</param>
	private List<string> SplitString(string str, int chunkSize)
	{
		var res = new List<string>();
		var len = str.Length;
		var i = 0;

		while (i < len)
		{
			var size = Math.Min(len - i, chunkSize);
			res.Add(str.Substring(i, size));
			i += size;
		}

		return res;
	}

	/// <summary>
	/// Gets the cancellation token.
	/// </summary>
	/// <param name="timeout">The timeout.</param>
	private CancellationToken GetCancellationToken(TimeSpan? timeout = null) => new CancellationTokenSource(timeout ?? this.Config.Timeout).Token;

	/// <summary>
	/// Handles an invalid interaction.
	/// </summary>
	/// <param name="interaction">The interaction.</param>
	private async Task HandleInvalidInteraction(DiscordInteraction interaction)
	{
		var at = this.Config.ResponseBehavior switch
		{
			InteractionResponseBehavior.Ack => interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate),
			InteractionResponseBehavior.Respond => interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
			{
				Content = this.Config.ResponseMessage,
				IsEphemeral = true
			}),
			InteractionResponseBehavior.Ignore => Task.CompletedTask,
			_ => throw new ArgumentException("Unknown enum value.")
		};

		await at.ConfigureAwait(false);
	}
}
