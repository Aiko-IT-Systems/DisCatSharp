// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp
{
    /// <summary>
    /// Represents a composite default logger.
    /// </summary>
    internal class CompositeDefaultLogger : ILogger<BaseDiscordClient>
    {
        /// <summary>
        /// Gets the loggers.
        /// </summary>
        private IEnumerable<ILogger<BaseDiscordClient>> Loggers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeDefaultLogger"/> class.
        /// </summary>
        /// <param name="Providers">The providers.</param>
        public CompositeDefaultLogger(IEnumerable<ILoggerProvider> Providers)
        {
            this.Loggers = Providers.Select(X => X.CreateLogger(typeof(BaseDiscordClient).FullName))
                .OfType<ILogger<BaseDiscordClient>>()
                .ToList();
        }

        /// <summary>
        /// Whether the logger is enabled.
        /// </summary>
        /// <param name="LogLevel">The log level.</param>
        public bool IsEnabled(LogLevel LogLevel)
            => true;

        /// <summary>
        /// Logs an event.
        /// </summary>
        /// <param name="LogLevel">The log level.</param>
        /// <param name="EventId">The event id.</param>
        /// <param name="State">The state.</param>
        /// <param name="Exception">The exception.</param>
        /// <param name="Formatter">The formatter.</param>
        public void Log<TState>(LogLevel LogLevel, EventId EventId, TState State, Exception Exception, Func<TState, Exception, string> Formatter)
        {
            foreach (var logger in this.Loggers)
                logger.Log(LogLevel, EventId, State, Exception, Formatter);
        }

        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <param name="State">The state.</param>
        public IDisposable BeginScope<TState>(TState State) => throw new NotImplementedException();
    }
}
