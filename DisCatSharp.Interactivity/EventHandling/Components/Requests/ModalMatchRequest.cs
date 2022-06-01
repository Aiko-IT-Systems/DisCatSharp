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
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.EventArgs;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// Represents a match that is being waited for.
/// </summary>
internal class ModalMatchRequest
{
	/// <summary>
	/// The id to wait on. This should be uniquely formatted to avoid collisions.
	/// </summary>
	public string CustomId { get; private set; }

	/// <summary>
	/// The completion source that represents the result of the match.
	/// </summary>
	public TaskCompletionSource<ComponentInteractionCreateEventArgs> Tcs { get; private set; } = new();

	protected readonly CancellationToken Cancellation;
	protected readonly Func<ComponentInteractionCreateEventArgs, bool> Predicate;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModalMatchRequest"/> class.
	/// </summary>
	/// <param name="customId">The custom id.</param>
	/// <param name="predicate">The predicate.</param>
	/// <param name="cancellation">The cancellation token.</param>
	public ModalMatchRequest(string customId, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken cancellation)
	{
		this.CustomId = customId;
		this.Predicate = predicate;
		this.Cancellation = cancellation;
		this.Cancellation.Register(() => this.Tcs.TrySetResult(null)); // TrySetCancelled would probably be better but I digress ~Velvet //
	}

	/// <summary>
	/// Whether it is a match.
	/// </summary>
	/// <param name="args">The arguments.</param>
	public bool IsMatch(ComponentInteractionCreateEventArgs args) => this.Predicate(args);
}
