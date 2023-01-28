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
using System.Linq;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a composite default logger.
/// </summary>
internal class CompositeDefaultLogger : ILogger<BaseDiscordClient>
{
	/// <summary>
	/// Gets the loggers.
	/// </summary>
	private readonly List<ILogger<BaseDiscordClient>> _loggers;

	/// <summary>
	/// Initializes a new instance of the <see cref="CompositeDefaultLogger"/> class.
	/// </summary>
	/// <param name="providers">The providers.</param>
	public CompositeDefaultLogger(IEnumerable<ILoggerProvider> providers)
	{
		this._loggers = providers.Select(x => x.CreateLogger(typeof(BaseDiscordClient).FullName))
			.OfType<ILogger<BaseDiscordClient>>()
			.ToList();
	}

	/// <summary>
	/// Whether the logger is enabled.
	/// </summary>
	/// <param name="logLevel">The log level.</param>
	public bool IsEnabled(LogLevel logLevel)
		=> true;

	/// <summary>
	/// Logs an event.
	/// </summary>
	/// <param name="logLevel">The log level.</param>
	/// <param name="eventId">The event id.</param>
	/// <param name="state">The state.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="formatter">The formatter.</param>
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		foreach (var logger in this._loggers)
			logger.Log(logLevel, eventId, state, exception, formatter);
	}

	/// <summary>
	/// Begins the scope.
	/// </summary>
	/// <param name="state">The state.</param>
	public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
}
