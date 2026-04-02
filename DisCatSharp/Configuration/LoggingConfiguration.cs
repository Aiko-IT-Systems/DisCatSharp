using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
///     Configuration for Discord client logging behavior.
/// </summary>
public sealed class LoggingConfiguration
{
	/// <summary>
	///     Creates a new logging configuration with default values.
	/// </summary>
	public LoggingConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another logging configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public LoggingConfiguration(LoggingConfiguration other)
	{
		this.MinimumLogLevel = other.MinimumLogLevel;
		this.LogTimestampFormat = other.LogTimestampFormat;
		this.LoggerFactory = other.LoggerFactory;
		this.HasShardLogger = other.HasShardLogger;
	}

	/// <summary>
	///     <para>Sets the minimum logging level for messages.</para>
	///     <para>Defaults to <see cref="LogLevel.Information" />.</para>
	/// </summary>
	public LogLevel MinimumLogLevel { internal get; set; } = LogLevel.Information;

	/// <summary>
	///     <para>Allows you to overwrite the time format used by the internal debug logger.</para>
	///     <para>
	///         Only applicable when <see cref="LoggerFactory" /> is set left at default value. Defaults to ISO 8601-like
	///         format.
	///     </para>
	/// </summary>
	public string LogTimestampFormat { internal get; set; } = "yyyy-MM-dd HH:mm:ss zzz";

	/// <summary>
	///     <para>Sets the logger implementation to use.</para>
	///     <para>To create your own logger, implement the <see cref="Microsoft.Extensions.Logging.ILoggerFactory" /> instance.</para>
	///     <para>Defaults to built-in implementation.</para>
	/// </summary>
	public ILoggerFactory LoggerFactory { internal get; set; } = null!;

	/// <summary>
	///     Sets whether a shard logger is attached.
	/// </summary>
	internal bool HasShardLogger { get; set; } = false;
}
