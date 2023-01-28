// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a default logger factory.
/// </summary>
internal class DefaultLoggerFactory : ILoggerFactory
{
	/// <summary>
	/// Gets the providers.
	/// </summary>
	private readonly List<ILoggerProvider> _providers = new();
	private bool _isDisposed;

	/// <summary>
	/// Adds a provider.
	/// </summary>
	/// <param name="provider">The provider to be added.</param>
	public void AddProvider(ILoggerProvider provider) => this._providers.Add(provider);

	/// <summary>
	/// Creates the logger.
	/// </summary>
	/// <param name="categoryName">The category name.</param>
	public ILogger CreateLogger(string categoryName) =>
		this._isDisposed
			? throw new InvalidOperationException("This logger factory is already disposed.")
			: categoryName != typeof(BaseDiscordClient).FullName && categoryName != typeof(DiscordWebhookClient).FullName
				? throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseDiscordClient).FullName} or {typeof(DiscordWebhookClient).FullName}.", nameof(categoryName))
				: new CompositeDefaultLogger(this._providers);

	/// <summary>
	/// Disposes the logger.
	/// </summary>
	public void Dispose()
	{
		if (this._isDisposed)
			return;
		this._isDisposed = true;

		foreach (var provider in this._providers)
			provider.Dispose();

		this._providers.Clear();
	}
}
