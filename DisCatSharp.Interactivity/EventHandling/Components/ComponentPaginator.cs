using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Telemetry;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     The component paginator.
/// </summary>
internal class ComponentPaginator : IPaginator
{
	private readonly DiscordClient _client;
	private readonly InteractivityConfiguration _config;
	private readonly ConcurrentDictionary<ulong, IPaginationRequest> _requests = [];
	private bool _disposed;

	/// <summary>
	///     Initializes a new instance of the <see cref="ComponentPaginator" /> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="config">The config.</param>
	public ComponentPaginator(DiscordClient client, InteractivityConfiguration config)
	{
		this._client = client;
		this._client.InternalComponentInteractionCreated.Register(this.Handle);
		this._config = config;
	}

	/// <summary>
	///     Does the pagination async.
	/// </summary>
	/// <param name="request">The request.</param>
	public async Task DoPaginationAsync(IPaginationRequest request)
	{
		var id = (await request.GetMessageAsync().ConfigureAwait(false)).Id;
		this._requests.TryAdd(id, request);

		try
		{
			var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
			await tcs.Task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", ex);
			this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "There was an exception while paginating.");
		}
		finally
		{
			this._requests.TryRemove(id, out _);
			try
			{
				await request.DoCleanupAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", ex);
				this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "There was an exception while cleaning up pagination.");
			}
		}
	}

	/// <summary>
	///     Disposes the paginator.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;
		this._client.InternalComponentInteractionCreated.Unregister(this.Handle);
		this._requests.Clear();
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	~ComponentPaginator()
	{
		this.Dispose();
	}

	/// <summary>
	///     Handles the pagination event.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The event arguments.</param>
	private async Task Handle(DiscordClient client, ComponentInteractionCreateEventArgs e)
	{
		if (e.Interaction.Type == InteractionType.ModalSubmit)
			return;

		if (!this._requests.TryGetValue(e.Message.Id, out var req))
			return;

		if (!this._config.PaginationButtons.ButtonArray.Select(x => x.CustomId).Contains(e.Id))
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
	///     Handles the pagination async.
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

		var page = await request.GetPageAsync().ConfigureAwait(false);

		var bts = await request.GetButtonsAsync().ConfigureAwait(false);

		if (request is InteractionPaginationRequest ipr)
		{
			var builder = new DiscordWebhookBuilder();
			if (id == buttons.Stop.CustomId)
			{
				if (!page.UsesCV2)
				{
					if (page.Content is not null)
						builder.WithContent(page.Content);
					if (page.Embed is not null)
						builder.AddEmbed(page.Embed);
					builder.AddComponents(bts);
				}
				else
				{
					builder.WithV2Components();
					builder.AddComponents(page.ComponentsInternal);
					builder.AddComponents(new DiscordActionRowComponent(bts));
					builder.DisableAllComponents();
				}

				builder.DisableAllComponents();
				await (await ipr.GetLastInteractionAsync()).EditOriginalResponseAsync(builder).ConfigureAwait(false);

				return;
			}

			if (!page.UsesCV2)
			{
				if (page.Content is not null)
					builder.WithContent(page.Content);
				if (page.Embed is not null)
					builder.AddEmbed(page.Embed);
				builder.AddComponents(bts);
			}
			else
			{
				builder.WithV2Components();
				builder.AddComponents(page.ComponentsInternal);
				builder.AddComponents(new DiscordActionRowComponent(bts));
			}

			await (await ipr.GetLastInteractionAsync()).EditOriginalResponseAsync(builder).ConfigureAwait(false);
			return;
		}

		var msgBuilder = new DiscordMessageBuilder();

		if (!page.UsesCV2)
		{
			if (page.Content is not null)
				msgBuilder.WithContent(page.Content);
			if (page.Embed is not null)
				msgBuilder.AddEmbed(page.Embed);
			msgBuilder.AddComponents(bts);
		}
		else
		{
			msgBuilder.WithV2Components();
			msgBuilder.AddComponents(page.ComponentsInternal);
			msgBuilder.AddComponents(new DiscordActionRowComponent(bts));
		}

		await msgBuilder.ModifyAsync(msg).ConfigureAwait(false);
	}
}
