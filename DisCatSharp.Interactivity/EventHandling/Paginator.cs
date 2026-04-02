using System;
using System.Threading;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Telemetry;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     The paginator.
/// </summary>
internal class Paginator : IPaginator
{
	private readonly DiscordClient _client;
	private bool _disposed;
	private readonly ConcurrentHashSet<IPaginationRequest> _requests;

	/// <summary>
	///     Creates a new EventWaiter object.
	/// </summary>
	/// <param name="client">Discord client</param>
	public Paginator(DiscordClient client)
	{
		this._client = client;
		this._requests = [];

		this._client.InternalMessageReactionAdded.Register(this.HandleReactionAdd);
		this._client.InternalMessageReactionRemoved.Register(this.HandleReactionRemove);
		this._client.InternalMessageReactionsCleared.Register(this.HandleReactionClear);
	}

	/// <summary>
	///     Dos the pagination async.
	/// </summary>
	/// <param name="request">The request.</param>
	public async Task DoPaginationAsync(IPaginationRequest request)
	{
		await this.ResetReactionsAsync(request).ConfigureAwait(false);
		this._requests.Add(request);
		try
		{
			var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
			await tcs.Task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", ex);
			this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
		}
		finally
		{
			this._requests.TryRemove(request);
			try
			{
				await request.DoCleanupAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", ex);
				this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
			}
		}
	}

	/// <summary>
	///     Disposes this EventWaiter
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;

		var client = this._client;
		if (client is not null)
		{
			client.InternalMessageReactionAdded.Unregister(this.HandleReactionAdd);
			client.InternalMessageReactionRemoved.Unregister(this.HandleReactionRemove);
			client.InternalMessageReactionsCleared.Unregister(this.HandleReactionClear);
		}

		this._requests?.Clear();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Handles the reaction add.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventArgs)
	{
		if (this._disposed || this._requests.Count == 0)
			return Task.CompletedTask;

		_ = Task.Run(async () =>
		{
			try
			{
				foreach (var req in this._requests)
				{
					var emojis = await req.GetEmojisAsync().ConfigureAwait(false);
					var msg = await req.GetMessageAsync().ConfigureAwait(false);
					var usr = await req.GetUserAsync().ConfigureAwait(false);

					if (msg.Id == eventArgs.Message.Id)
					{
						if (eventArgs.User.Id == usr.Id)
						{
							if (req.PageCount > 1 &&
								(eventArgs.Emoji == emojis.Left ||
								 eventArgs.Emoji == emojis.SkipLeft ||
								 eventArgs.Emoji == emojis.Right ||
								 eventArgs.Emoji == emojis.SkipRight ||
								 eventArgs.Emoji == emojis.Stop))
								await this.PaginateAsync(req, eventArgs.Emoji).ConfigureAwait(false);
							else if (eventArgs.Emoji == emojis.Stop &&
									 req is PaginationRequest { PaginationDeletion: PaginationDeletion.DeleteMessage })
								await this.PaginateAsync(req, eventArgs.Emoji).ConfigureAwait(false);
							else
								await msg.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User).ConfigureAwait(false);
						}
						else if (eventArgs.User.Id != this._client.CurrentUser.Id)
							if (eventArgs.Emoji != emojis.Left &&
								eventArgs.Emoji != emojis.SkipLeft &&
								eventArgs.Emoji != emojis.Right &&
								eventArgs.Emoji != emojis.SkipRight &&
								eventArgs.Emoji != emojis.Stop)
								await msg.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				this._client?.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while handling reaction add in paginator");
			}
		});
		return Task.CompletedTask;
	}

	/// <summary>
	///     Handles the reaction remove.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
	{
		if (this._disposed || this._requests.Count == 0)
			return Task.CompletedTask;

		_ = Task.Run(async () =>
		{
			try
			{
				foreach (var req in this._requests)
				{
					var emojis = await req.GetEmojisAsync().ConfigureAwait(false);
					var msg = await req.GetMessageAsync().ConfigureAwait(false);
					var usr = await req.GetUserAsync().ConfigureAwait(false);

					if (msg.Id == eventArgs.Message.Id)
						if (eventArgs.User.Id == usr.Id)
						{
							if (req.PageCount > 1 &&
								(eventArgs.Emoji == emojis.Left ||
								 eventArgs.Emoji == emojis.SkipLeft ||
								 eventArgs.Emoji == emojis.Right ||
								 eventArgs.Emoji == emojis.SkipRight ||
								 eventArgs.Emoji == emojis.Stop))
								await this.PaginateAsync(req, eventArgs.Emoji).ConfigureAwait(false);
							else if (eventArgs.Emoji == emojis.Stop &&
									 req is PaginationRequest { PaginationDeletion: PaginationDeletion.DeleteMessage })
								await this.PaginateAsync(req, eventArgs.Emoji).ConfigureAwait(false);
						}
				}
			}
			catch (Exception ex)
			{
				this._client?.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while handling reaction remove in paginator");
			}
		});

		return Task.CompletedTask;
	}

	/// <summary>
	///     Handles the reaction clear.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The eventArgs.</param>
	private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventArgs)
	{
		if (this._disposed || this._requests.Count == 0)
			return Task.CompletedTask;

		_ = Task.Run(async () =>
		{
			try
			{
				foreach (var req in this._requests)
				{
					var msg = await req.GetMessageAsync().ConfigureAwait(false);

					if (msg.Id == eventArgs.Message.Id)
						await this.ResetReactionsAsync(req).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				this._client?.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while handling reaction clear in paginator");
			}
		});

		return Task.CompletedTask;
	}

	/// <summary>
	///     Resets the reactions async.
	/// </summary>
	/// <param name="p">The p.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	private async Task ResetReactionsAsync(IPaginationRequest p, CancellationToken cancellationToken = default)
	{
		var msg = await p.GetMessageAsync().ConfigureAwait(false);
		var emojis = await p.GetEmojisAsync().ConfigureAwait(false);

		var chn = msg.Channel;
		var gld = chn.Guild;
		var mbr = gld?.CurrentMember;

		if (mbr is not null && (chn.PermissionsFor(mbr) & Permissions.ManageMessages) != 0)
			await msg.DeleteAllReactionsAsync("Pagination", cancellationToken).ConfigureAwait(false);

		if (p.PageCount > 1)
		{
			if (emojis.SkipLeft is not null)
				await msg.CreateReactionAsync(emojis.SkipLeft, cancellationToken).ConfigureAwait(false);
			if (emojis.Left is not null)
				await msg.CreateReactionAsync(emojis.Left, cancellationToken).ConfigureAwait(false);
			if (emojis.Right is not null)
				await msg.CreateReactionAsync(emojis.Right, cancellationToken).ConfigureAwait(false);
			if (emojis.SkipRight is not null)
				await msg.CreateReactionAsync(emojis.SkipRight, cancellationToken).ConfigureAwait(false);
			if (emojis.Stop is not null)
				await msg.CreateReactionAsync(emojis.Stop, cancellationToken).ConfigureAwait(false);
		}
		else if (emojis.Stop is not null && p is PaginationRequest { PaginationDeletion: PaginationDeletion.DeleteMessage })
			await msg.CreateReactionAsync(emojis.Stop, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Paginates the async.
	/// </summary>
	/// <param name="p">The p.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	private async Task PaginateAsync(IPaginationRequest p, DiscordEmoji emoji, CancellationToken cancellationToken = default)
	{
		var emojis = await p.GetEmojisAsync().ConfigureAwait(false);
		var msg = await p.GetMessageAsync().ConfigureAwait(false);

		if (emoji == emojis.SkipLeft)
			await p.SkipLeftAsync().ConfigureAwait(false);
		else if (emoji == emojis.Left)
			await p.PreviousPageAsync().ConfigureAwait(false);
		else if (emoji == emojis.Right)
			await p.NextPageAsync().ConfigureAwait(false);
		else if (emoji == emojis.SkipRight)
			await p.SkipRightAsync().ConfigureAwait(false);
		else if (emoji == emojis.Stop)
		{
			var tcs = await p.GetTaskCompletionSourceAsync().ConfigureAwait(false);
			tcs.TrySetResult(true);
			return;
		}

		var page = await p.GetPageAsync().ConfigureAwait(false);
		var builder = new DiscordMessageBuilder();
		if (page.Content is not null)
			builder.WithContent(page.Content);
		if (page.Embed is not null)
			builder.AddEmbed(page.Embed);

		await msg.ModifyAsync(builder, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	~Paginator()
	{
		this.Dispose();
	}
}
