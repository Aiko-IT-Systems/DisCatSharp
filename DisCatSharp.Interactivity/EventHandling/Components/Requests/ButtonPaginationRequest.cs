using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Interactivity.Enums;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// The button pagination request.
/// </summary>
internal class ButtonPaginationRequest : IPaginationRequest
{
	private int _index;
	private readonly List<Page> _pages = [];

	private readonly TaskCompletionSource<bool> _tcs = new();

	private readonly CancellationToken _token;
	private readonly DiscordUser _user;
	private readonly DiscordMessage _message;
	private readonly PaginationButtons _buttons;
	private readonly PaginationBehaviour _wrapBehavior;
	private readonly ButtonPaginationBehavior _behaviorBehavior;

	/// <summary>
	/// Initializes a new instance of the <see cref="ButtonPaginationRequest"/> class.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <param name="user">The user.</param>
	/// <param name="behavior">The behavior.</param>
	/// <param name="buttonBehavior">The button behavior.</param>
	/// <param name="buttons">The buttons.</param>
	/// <param name="pages">The pages.</param>
	/// <param name="token">The token.</param>
	public ButtonPaginationRequest(
		DiscordMessage message,
		DiscordUser user,
		PaginationBehaviour behavior,
		ButtonPaginationBehavior buttonBehavior,
		PaginationButtons buttons,
		IEnumerable<Page> pages,
		CancellationToken token
	)
	{
		this._user = user;
		this._token = token;
		this._buttons = new(buttons);
		this._message = message;
		this._wrapBehavior = behavior;
		this._behaviorBehavior = buttonBehavior;
		this._pages.AddRange(pages);

		this._token.Register(() => this._tcs.TrySetResult(false));
	}

	/// <summary>
	/// Gets the page count.
	/// </summary>
	public int PageCount => this._pages.Count;

	/// <summary>
	/// Gets the page.
	/// </summary>
	public Task<Page> GetPageAsync()
	{
		var page = Task.FromResult(this._pages[this._index]);

		if (this.PageCount is 1)
		{
			this._buttons.SkipLeft.Disable();
			this._buttons.Left.Disable();
			this._buttons.Right.Disable();
			this._buttons.SkipRight.Disable();
			this._buttons.Stop.Enable();
			return page;
		}

		if (this._wrapBehavior is PaginationBehaviour.WrapAround)
			return page;

		this._buttons.SkipLeft.Disabled = this._index < 2;

		this._buttons.Left.Disabled = this._index < 1;

		this._buttons.Right.Disabled = this._index >= this.PageCount - 1;

		this._buttons.SkipRight.Disabled = this._index >= this.PageCount - 2;

		return page;
	}

	/// <summary>
	/// Skips the left.
	/// </summary>
	public Task SkipLeftAsync()
	{
		if (this._wrapBehavior is PaginationBehaviour.WrapAround)
		{
			this._index = this._index is 0 ? this._pages.Count - 1 : 0;
			return Task.CompletedTask;
		}

		this._index = 0;

		return Task.CompletedTask;
	}

	/// <summary>
	/// Skips the right.
	/// </summary>
	public Task SkipRightAsync()
	{
		if (this._wrapBehavior is PaginationBehaviour.WrapAround)
		{
			this._index = this._index == this.PageCount - 1 ? 0 : this.PageCount - 1;
			return Task.CompletedTask;
		}

		this._index = this._pages.Count - 1;

		return Task.CompletedTask;
	}

	/// <summary>
	/// Gets the next page.
	/// </summary>
	public Task NextPageAsync()
	{
		this._index++;

		if (this._wrapBehavior is PaginationBehaviour.WrapAround)
		{
			if (this._index >= this.PageCount)
				this._index = 0;

			return Task.CompletedTask;
		}

		this._index = Math.Min(this._index, this.PageCount - 1);

		return Task.CompletedTask;
	}

	/// <summary>
	/// Gets the previous page.
	/// </summary>
	public Task PreviousPageAsync()
	{
		this._index--;

		if (this._wrapBehavior is PaginationBehaviour.WrapAround)
		{
			if (this._index is -1)
				this._index = this._pages.Count - 1;

			return Task.CompletedTask;
		}

		this._index = Math.Max(this._index, 0);

		return Task.CompletedTask;
	}

	/// <summary>
	/// Gets the emojis.
	/// </summary>
	public Task<PaginationEmojis> GetEmojisAsync() => Task.FromException<PaginationEmojis>(new NotSupportedException("Emojis aren't supported for this request."));

	/// <summary>
	/// Gets the buttons.
	/// </summary>
	public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
		=> Task.FromResult((IEnumerable<DiscordButtonComponent>)this._buttons.ButtonArray);

	/// <summary>
	/// Gets the message.
	/// </summary>
	public Task<DiscordMessage> GetMessageAsync() => Task.FromResult(this._message);

	/// <summary>
	/// Gets the user.
	/// </summary>
	public Task<DiscordUser> GetUserAsync() => Task.FromResult(this._user);

	/// <summary>
	/// Gets the task completion source.
	/// </summary>
	public Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync() => Task.FromResult(this._tcs);

	/// <summary>
	/// Does the cleanup.
	/// </summary>
	public async Task DoCleanupAsync()
	{
		switch (this._behaviorBehavior)
		{
			case ButtonPaginationBehavior.Disable:
				var buttons = this._buttons.ButtonArray.Select(b => b.Disable());

				var builder = new DiscordMessageBuilder()
					.WithContent(this._pages[this._index].Content)
					.AddEmbed(this._pages[this._index].Embed)
					.AddComponents(buttons);

				await builder.ModifyAsync(this._message).ConfigureAwait(false);
				break;

			case ButtonPaginationBehavior.DeleteButtons:
				builder = new DiscordMessageBuilder()
					.WithContent(this._pages[this._index].Content)
					.AddEmbed(this._pages[this._index].Embed);

				await builder.ModifyAsync(this._message).ConfigureAwait(false);
				break;

			case ButtonPaginationBehavior.DeleteMessage:
				await this._message.DeleteAsync().ConfigureAwait(false);
				break;

			case ButtonPaginationBehavior.Ignore:
				break;
		}
	}
}
