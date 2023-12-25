using System;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a default logger.
/// </summary>
public class DefaultLogger : ILogger<BaseDiscordClient>
{
	private static readonly object s_lock = new();

	/// <summary>
	/// Gets the minimum log level.
	/// </summary>
	private readonly LogLevel _minimumLevel;

	/// <summary>
	/// Gets the timestamp format.
	/// </summary>
	private readonly string _timestampFormat;

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultLogger"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal DefaultLogger(BaseDiscordClient client)
		: this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultLogger"/> class.
	/// </summary>
	/// <param name="minLevel">The min level.</param>
	/// <param name="timestampFormat">The timestamp format.</param>
	internal DefaultLogger(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
	{
		this._minimumLevel = minLevel;
		this._timestampFormat = timestampFormat;
	}

	/// <summary>
	/// Logs an event.
	/// </summary>
	/// <param name="logLevel">The log level.</param>
	/// <param name="eventId">The event id.</param>
	/// <param name="state">The state.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="formatter">The formatter.</param>
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!this.IsEnabled(logLevel))
			return;

		lock (s_lock)
		{
			var ename = eventId.Name;
			ename = ename?.Length > 12 ? ename?[..12] : ename;
			Console.Write($"[{DateTimeOffset.Now.ToString(this._timestampFormat)}] [{eventId.Id,-4}/{ename,-12}] ");

			switch (logLevel)
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
				case LogLevel.None:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}

			Console.Write(logLevel switch
			{
				LogLevel.Trace => "[Trace] ",
				LogLevel.Debug => "[Debug] ",
				LogLevel.Information => "[Info ] ",
				LogLevel.Warning => "[Warn ] ",
				LogLevel.Error => "[Error] ",
				LogLevel.Critical => "[Critical ]",
				LogLevel.None => "[None ] ",
				_ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
			});
			Console.ResetColor();

			//The foreground color is off.
			if (logLevel is LogLevel.Critical)
				Console.Write(" ");

			var message = formatter(state, exception);
			Console.WriteLine(message);
			if (exception is not null)
				Console.WriteLine(exception);
		}
	}

	/// <summary>
	/// Whether the logger is enabled.
	/// </summary>
	/// <param name="logLevel">The log level.</param>
	public bool IsEnabled(LogLevel logLevel)
		=> logLevel >= this._minimumLevel;

	/// <summary>
	/// Begins the scope.
	/// </summary>
	/// <param name="state">The state.</param>
	/// <returns>An IDisposable.</returns>
	public IDisposable BeginScope<TState>(TState state) where TState : notnull
		=> throw new NotImplementedException();
}
