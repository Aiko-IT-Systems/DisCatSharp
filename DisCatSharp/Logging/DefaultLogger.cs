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
using Microsoft.Extensions.Logging;

namespace DisCatSharp
{
    /// <summary>
    /// Represents a default logger.
    /// </summary>
    public class DefaultLogger : ILogger<BaseDiscordClient>
    {
        private static readonly object _lock = new();

        /// <summary>
        /// Gets the minimum log level.
        /// </summary>
        private LogLevel MinimumLevel { get; }
        /// <summary>
        /// Gets the timestamp format.
        /// </summary>
        private string TimestampFormat { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogger"/> class.
        /// </summary>
        /// <param name="Client">The client.</param>
        internal DefaultLogger(BaseDiscordClient Client)
            : this(Client.Configuration.MinimumLogLevel, Client.Configuration.LogTimestampFormat)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogger"/> class.
        /// </summary>
        /// <param name="MinLevel">The min level.</param>
        /// <param name="TimestampFormat">The timestamp format.</param>
        internal DefaultLogger(LogLevel MinLevel = LogLevel.Information, string TimestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
        {
            this.MinimumLevel = MinLevel;
            this.TimestampFormat = TimestampFormat;
        }

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
            if (!this.IsEnabled(LogLevel))
                return;

            lock (_lock)
            {
                var ename = EventId.Name;
                ename = ename?.Length > 12 ? ename?[..12] : ename;
                Console.Write($"[{DateTimeOffset.Now.ToString(this.TimestampFormat)}] [{EventId.Id,-4}/{ename,-12}] ");

                switch (LogLevel)
                {
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;

                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        break;

                    case LogLevel.Information:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;

                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case LogLevel.Critical:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }
                Console.Write(LogLevel switch
                {
                    LogLevel.Trace => "[Trace] ",
                    LogLevel.Debug => "[Debug] ",
                    LogLevel.Information => "[Info ] ",
                    LogLevel.Warning => "[Warn ] ",
                    LogLevel.Error => "[Error] ",
                    LogLevel.Critical => "[Crit ]",
                    LogLevel.None => "[None ] ",
                    _ => "[?????] "
                });
                Console.ResetColor();

                //The foreground color is off.
                if (LogLevel == LogLevel.Critical)
                    Console.Write(" ");

                var message = Formatter(State, Exception);
                Console.WriteLine(message);
                if (Exception != null)
                    Console.WriteLine(Exception);
            }
        }

        /// <summary>
        /// Whether the logger is enabled.
        /// </summary>
        /// <param name="LogLevel">The log level.</param>
        public bool IsEnabled(LogLevel LogLevel)
            => LogLevel >= this.MinimumLevel;

        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <param name="State">The state.</param>
        /// <returns>An IDisposable.</returns>
        public IDisposable BeginScope<TState>(TState State) => throw new NotImplementedException();
    }
}
