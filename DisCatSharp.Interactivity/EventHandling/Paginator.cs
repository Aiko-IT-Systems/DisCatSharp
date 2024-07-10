using System;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// The paginator.
/// </summary>
internal class Paginator : IPaginator
{
	private DiscordClient _client;
	private ConcurrentHashSet<IPaginationRequest> _requests;

	/// <summary>
	/// Creates a new EventWaiter object.
	/// </summary>
	/// <param name="client">Discord client</param>
	public Paginator(DiscordClient client)
	{
		this._client = client;
		this._requests = [];

		this._client.MessageReactionAdded += this.HandleReactionAdd;
		this._client.MessageReactionRemoved += this.HandleReactionRemove;
		this._client.MessageReactionsCleared += this.HandleReactionClear;
	}

	/// <summary>
	/// Dos the pagination async.
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
				this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
			}
		}
	}

	/// <summary>
	/// Handles the reaction add.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventArgs)
	{
		if (this._requests.Count == 0)
			return Task.CompletedTask;

		_ = Task.Run(async () =>
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
		});
		return Task.CompletedTask;
	}

	/// <summary>
	/// Handles the reaction remove.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
	{
		if (this._requests.Count == 0)
			return Task.CompletedTask;

		_ = Task.Run(async () =>
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
		});

		return Task.CompletedTask;
	}

	/// <summary>
	/// Handles the reaction clear.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The eventArgs.</param>
	private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventArgs)
	{
		if (this._requests.Count == 0)
			return Task.CompletedTask;

		_ = Task.Run(async () =>
		{
			foreach (var req in this._requests)
			{
				var msg = await req.GetMessageAsync().ConfigureAwait(false);

				if (msg.Id == eventArgs.Message.Id)
					await this.ResetReactionsAsync(req).ConfigureAwait(false);
			}
		});

		return Task.CompletedTask;
	}

	/// <summary>
	/// Resets the reactions async.
	/// </summary>
	/// <param name="p">The p.</param>
	private async Task ResetReactionsAsync(IPaginationRequest p)
	{
		var msg = await p.GetMessageAsync().ConfigureAwait(false);
		var emojis = await p.GetEmojisAsync().ConfigureAwait(false);

		var chn = msg.Channel;
		var gld = chn?.Guild;
		var mbr = gld?.CurrentMember;

		if (mbr != null && (chn.PermissionsFor(mbr) & Permissions.ManageMessages) != 0)
			await msg.DeleteAllReactionsAsync("Pagination").ConfigureAwait(false);

		if (p.PageCount > 1)
		{
			if (emojis.SkipLeft != null)
				await msg.CreateReactionAsync(emojis.SkipLeft).ConfigureAwait(false);
			if (emojis.Left != null)
				await msg.CreateReactionAsync(emojis.Left).ConfigureAwait(false);
			if (emojis.Right != null)
				await msg.CreateReactionAsync(emojis.Right).ConfigureAwait(false);
			if (emojis.SkipRight != null)
				await msg.CreateReactionAsync(emojis.SkipRight).ConfigureAwait(false);
			if (emojis.Stop != null)
				await msg.CreateReactionAsync(emojis.Stop).ConfigureAwait(false);
		}
		else if (emojis.Stop != null && p is PaginationRequest { PaginationDeletion: PaginationDeletion.DeleteMessage })
			await msg.CreateReactionAsync(emojis.Stop).ConfigureAwait(false);
	}

	/// <summary>
	/// Paginates the async.
	/// </summary>
	/// <param name="p">The p.</param>
	/// <param name="emoji">The emoji.</param>
	private async Task PaginateAsync(IPaginationRequest p, DiscordEmoji emoji)
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
		var builder = new DiscordMessageBuilder()
			.WithContent(page.Content)
			.WithEmbed(page.Embed);

		await builder.ModifyAsync(msg).ConfigureAwait(false);
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
