using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Interactivity.Enums;

namespace DisCatSharp.Interactivity.EventHandling
{
	/// <summary>
	/// The pagination request.
	/// </summary>
	internal class PaginationRequest : IPaginationRequest
	{
		private TaskCompletionSource<bool>? _tcs;
		private readonly CancellationTokenSource _ct;
		private readonly TimeSpan _timeout;
		private readonly List<Page> _pages;
		private readonly PaginationBehaviour _behaviour;
		private readonly DiscordMessage _message;
		private readonly PaginationEmojis _emojis;
		private readonly DiscordUser _user;
		private int _index;

		/// <summary>
		/// Creates a new Pagination request
		/// </summary>
		/// <param name="message">Message to paginate</param>
		/// <param name="user">User to allow control for</param>
		/// <param name="behaviour">Behaviour during pagination</param>
		/// <param name="deletion">Behavior on pagination end</param>
		/// <param name="emojis">Emojis for this pagination object</param>
		/// <param name="timeout">Timeout time</param>
		/// <param name="pages">Pagination pages</param>
		internal PaginationRequest(DiscordMessage message, DiscordUser user, PaginationBehaviour behaviour,
		                           PaginationDeletion deletion,
		                           PaginationEmojis emojis, TimeSpan timeout, IEnumerable<Page> pages)
		{
			this._tcs = new();
			this._ct = new(timeout);
			this._ct.Token.Register(() => this._tcs.TrySetResult(true));
			this._timeout = timeout;

			this._message = message;
			this._user = user;

			this.PaginationDeletion = deletion;
			this._behaviour = behaviour;
			this._emojis = emojis;

			this._pages = new();
			foreach (var p in pages) this._pages.Add(p);
		}

		/// <summary>
		/// Gets the page count.
		/// </summary>
		public int PageCount
			=> this._pages.Count;

		/// <summary>
		/// Gets the pagination deletion.
		/// </summary>
		public PaginationDeletion PaginationDeletion { get; }

		/// <summary>
		/// Gets the page.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task<Page> GetPageAsync()
		{
			await Task.Yield();

			return this._pages[this._index];
		}

		/// <summary>
		/// Skips the to the left.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task SkipLeftAsync()
		{
			await Task.Yield();

			this._index = 0;
		}

		/// <summary>
		/// Skips to the right.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task SkipRightAsync()
		{
			await Task.Yield();

			this._index = this._pages.Count - 1;
		}

		/// <summary>
		/// Goes to the next page.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task NextPageAsync()
		{
			await Task.Yield();

			switch (this._behaviour)
			{
				case PaginationBehaviour.Ignore:
					if (this._index != this._pages.Count - 1)
						this._index++;
					break;

				case PaginationBehaviour.WrapAround:
					if (this._index == this._pages.Count - 1)
						this._index = 0;
					else
						this._index++;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Goes to the previous page.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task PreviousPageAsync()
		{
			await Task.Yield();

			switch (this._behaviour)
			{
				case PaginationBehaviour.Ignore:
					if (this._index != 0)
						this._index--;

					break;

				case PaginationBehaviour.WrapAround:
					if (this._index == 0)
						this._index = this._pages.Count - 1;
					else
						this._index--;

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Gets the buttons.
		/// </summary>
		/// <returns><see cref="NotSupportedException"/></returns>
		public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
			=> throw new NotSupportedException("This request does not support buttons.");

		/// <summary>
		/// Gets the emojis.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task<PaginationEmojis> GetEmojisAsync()
		{
			await Task.Yield();

			return this._emojis;
		}

		/// <summary>
		/// Gets the message.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task<DiscordMessage> GetMessageAsync()
		{
			await Task.Yield();

			return this._message;
		}

		/// <summary>
		/// Gets the user.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task<DiscordUser> GetUserAsync()
		{
			await Task.Yield();

			return this._user;
		}

		/// <summary>
		/// Does the cleanup.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task DoCleanupAsync()
		{
			switch (this.PaginationDeletion)
			{
				case PaginationDeletion.DeleteEmojis:
					await this._message.DeleteAllReactionsAsync().ConfigureAwait(false);
					break;

				case PaginationDeletion.DeleteMessage:
					await this._message.DeleteAsync().ConfigureAwait(false);
					break;

				case PaginationDeletion.KeepEmojis:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Gets the task completion source.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task<TaskCompletionSource<bool>?> GetTaskCompletionSourceAsync()
		{
			await Task.Yield();

			return this._tcs;
		}

		~PaginationRequest()
		{
			this.Dispose();
		}

		/// <summary>
		/// Disposes this PaginationRequest.
		/// </summary>
		public void Dispose()
		{
			this._ct.Dispose();
			this._tcs = null;
		}
	}
}

namespace DisCatSharp.Interactivity
{
	/// <summary>
	/// The pagination emojis.
	/// </summary>
	public class PaginationEmojis
	{
		public readonly DiscordEmoji? SkipLeft;
		public readonly DiscordEmoji? SkipRight;
		public readonly DiscordEmoji? Left;
		public readonly DiscordEmoji? Right;
		public readonly DiscordEmoji? Stop;

		/// <summary>
		/// Initializes a new instance of the <see cref="PaginationEmojis"/> class.
		/// </summary>
		public PaginationEmojis()
		{
			this.Left = DiscordEmoji.FromUnicode("◀");
			this.Right = DiscordEmoji.FromUnicode("▶");
			this.SkipLeft = DiscordEmoji.FromUnicode("⏮");
			this.SkipRight = DiscordEmoji.FromUnicode("⏭");
			this.Stop = DiscordEmoji.FromUnicode("⏹");
		}
	}

	/// <summary>
	/// The page.
	/// </summary>
	public class Page
	{
		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		public string? Content { get; set; }

		/// <summary>
		/// Gets or sets the embed.
		/// </summary>
		public DiscordEmbed? Embed { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Page"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="embed">The embed.</param>
		public Page(string? content = null, DiscordEmbedBuilder? embed = null)
		{
			this.Content = content;
			this.Embed = embed?.Build();
		}
	}
}
