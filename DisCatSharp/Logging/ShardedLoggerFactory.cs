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

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a sharded logger factory.
/// </summary>
internal class ShardedLoggerFactory : ILoggerFactory
{
	/// <summary>
	/// Gets the logger.
	/// </summary>
	private readonly ILogger<BaseDiscordClient> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="ShardedLoggerFactory"/> class.
	/// </summary>
	/// <param name="instance">The instance.</param>
	public ShardedLoggerFactory(ILogger<BaseDiscordClient> instance)
	{
		this._logger = instance;
	}

	/// <summary>
	/// Adds a provider.
	/// </summary>
	/// <param name="provider">The provider to be added.</param>
	public void AddProvider(ILoggerProvider provider) => throw new InvalidOperationException("This is a passthrough logger container, it cannot register new providers.");

	/// <summary>
	/// Creates a logger.
	/// </summary>
	/// <param name="categoryName">The category name.</param>
	public ILogger CreateLogger(string categoryName) =>
		categoryName != typeof(BaseDiscordClient).FullName
			? throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseDiscordClient).FullName}.", nameof(categoryName))
			: this._logger;

	/// <summary>
	/// Disposes the logger.
	/// </summary>
	public void Dispose()
	{ }
}
