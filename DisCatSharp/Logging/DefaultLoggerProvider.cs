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
/// Represents a default logger provider.
/// </summary>
internal class DefaultLoggerProvider : ILoggerProvider
{
	/// <summary>
	/// Gets the minimum log level.
	/// </summary>
	private readonly LogLevel _minimumLevel;

	/// <summary>
	/// Gets the timestamp format.
	/// </summary>
	private readonly string _timestampFormat;

	private bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultLoggerProvider"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal DefaultLoggerProvider(BaseDiscordClient client)
		: this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultLoggerProvider"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal DefaultLoggerProvider(DiscordWebhookClient client)
		: this(client.MinimumLogLevel, client.LogTimestampFormat)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultLoggerProvider"/> class.
	/// </summary>
	/// <param name="minLevel">The min level.</param>
	/// <param name="timestampFormat">The timestamp format.</param>
	internal DefaultLoggerProvider(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
	{
		this._minimumLevel = minLevel;
		this._timestampFormat = timestampFormat;
	}

	/// <summary>
	/// Creates the logger.
	/// </summary>
	/// <param name="categoryName">The category name.</param>
	public ILogger CreateLogger(string categoryName) =>
		this._isDisposed
			? throw new InvalidOperationException("This logger provider is already disposed.")
			: categoryName != typeof(BaseDiscordClient).FullName && categoryName != typeof(DiscordWebhookClient).FullName
				? throw new ArgumentException($"This provider can only provide instances of loggers for {typeof(BaseDiscordClient).FullName} or {typeof(DiscordWebhookClient).FullName}.", nameof(categoryName))
				: new DefaultLogger(this._minimumLevel, this._timestampFormat);

	/// <summary>
	/// Disposes the logger.
	/// </summary>
	public void Dispose() => this._isDisposed = true;
}
