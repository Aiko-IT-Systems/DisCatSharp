// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
using System.Linq;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// A modal-based version of <see cref="EventWaiter{T}"/>
/// </summary>
internal class ModalEventWaiter : IDisposable
{
	private readonly DiscordClient _client;
	private readonly ConcurrentHashSet<ModalMatchRequest> _modalMatchRequests = new();

	private readonly DiscordFollowupMessageBuilder _message;
	private readonly InteractivityConfiguration _config;

	/// <summary>
	/// Initializes a new instance of the <see cref="ComponentEventWaiter"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="config">The config.</param>
	public ModalEventWaiter(DiscordClient client, InteractivityConfiguration config)
	{
		this._client = client;
		this._client.ComponentInteractionCreated += this.Handle;
		this._config = config;

		this._message = new DiscordFollowupMessageBuilder { Content = config.ResponseMessage ?? "This modal was not meant for you.", IsEphemeral = true };
	}

	/// <summary>
	/// Waits for a specified <see cref="ModalMatchRequest"/>'s predicate to be fulfilled.
	/// </summary>
	/// <param name="request">The request to wait for.</param>
	/// <returns>The returned args, or null if it timed out.</returns>
	public async Task<ComponentInteractionCreateEventArgs> WaitForModalMatchAsync(ModalMatchRequest request)
	{
		this._modalMatchRequests.Add(request);

		try
		{
			return await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			this._client.Logger.LogError(InteractivityEvents.InteractivityWaitError, e, "An exception was thrown while waiting for modals.");
			return null;
		}
		finally
		{
			this._modalMatchRequests.TryRemove(request);
		}
	}

	/// <summary>
	/// Handles the waiter.
	/// </summary>
	/// <param name="_">The client.</param>
	/// <param name="args">The args.</param>
	private async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs args)
	{
		foreach (var mreq in this._modalMatchRequests.ToArray())
		{
			if (mreq.CustomId == args.Interaction.Data.CustomId && mreq.IsMatch(args))
				mreq.Tcs.TrySetResult(args);

			else if (this._config.ResponseBehavior is InteractionResponseBehavior.Respond)
				await args.Interaction.CreateFollowupMessageAsync(this._message).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Disposes the waiter.
	/// </summary>
	public void Dispose()
	{
		this._modalMatchRequests.Clear();
		this._client.ComponentInteractionCreated -= this.Handle;
	}
}
