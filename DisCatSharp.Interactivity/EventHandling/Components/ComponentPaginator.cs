using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// The component paginator.
/// </summary>
internal class ComponentPaginator : IPaginator
{
	private readonly DiscordClient _client;
	private readonly InteractivityConfiguration _config;
	private readonly DiscordMessageBuilder _builder = new();
	private readonly Dictionary<ulong, IPaginationRequest> _requests = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="ComponentPaginator"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="config">The config.</param>
	public ComponentPaginator(DiscordClient client, InteractivityConfiguration config)
	{
		this._client = client;
		this._client.ComponentInteractionCreated += this.Handle;
		this._config = config;
	}

	/// <summary>
	/// Does the pagination async.
	/// </summary>
	/// <param name="request">The request.</param>
	public async Task DoPaginationAsync(IPaginationRequest request)
	{
		var id = (await request.GetMessageAsync().ConfigureAwait(false)).Id;
		this._requests.Add(id, request);

		try
		{
			var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
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
				await request.DoCleanupAsync().ConfigureAwait(false);
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
	/// <param name="e">The event arguments.</param>
	private async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs e)
	{
		if (e.Interaction.Type == InteractionType.ModalSubmit)
			return;

		if (!this._requests.TryGetValue(e.Message.Id, out var req))
			return;

		if (this._config.AckPaginationButtons)
		{
			e.Handled = true;
			await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
		}

		if (await req.GetUserAsync().ConfigureAwait(false) != e.User)
		{
			if (this._config.ResponseBehavior is InteractionResponseBehavior.Respond)
				await e.Interaction.CreateFollowupMessageAsync(new()
				{
					Content = this._config.ResponseMessage,
					IsEphemeral = true
				}).ConfigureAwait(false);

			return;
		}

		if (req is InteractionPaginationRequest ipr)
			ipr.RegenerateCts(e.Interaction); // Necessary to ensure we don't prematurely yeet the CTS //

		await this.HandlePaginationAsync(req, e).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the pagination async.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="args">The arguments.</param>
	private async Task HandlePaginationAsync(IPaginationRequest request, ComponentInteractionCreateEventArgs args)
	{
		var buttons = this._config.PaginationButtons;
		var msg = await request.GetMessageAsync().ConfigureAwait(false);
		var id = args.Id;
		var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);

		var paginationTask = id switch
		{
			_ when id == buttons.SkipLeft.CustomId => request.SkipLeftAsync(),
			_ when id == buttons.SkipRight.CustomId => request.SkipRightAsync(),
			_ when id == buttons.Stop.CustomId => Task.FromResult(tcs.TrySetResult(true)),
			_ when id == buttons.Left.CustomId => request.PreviousPageAsync(),
			_ when id == buttons.Right.CustomId => request.NextPageAsync(),
			_ => throw new ArgumentOutOfRangeException(nameof(args), "Id was out of range")
		};

		await paginationTask.ConfigureAwait(false);

		if (id == buttons.Stop.CustomId)
			return;

		var page = await request.GetPageAsync().ConfigureAwait(false);

		var bts = await request.GetButtonsAsync().ConfigureAwait(false);

		if (request is InteractionPaginationRequest ipr)
		{
			var builder = new DiscordWebhookBuilder()
				.WithContent(page.Content)
				.AddEmbed(page.Embed)
				.AddComponents(bts);

			await args.Interaction.EditOriginalResponseAsync(builder).ConfigureAwait(false);
			return;
		}

		this._builder.Clear();

		this._builder
			.WithContent(page.Content)
			.AddEmbed(page.Embed)
			.AddComponents(bts);

		await this._builder.ModifyAsync(msg).ConfigureAwait(false);
	}
}
