using System;

using DisCatSharp.Telemetry;

using Sentry.Extensibility;

namespace DisCatSharp.Exceptions;

/// <summary>
///     Represents a filter for exceptions that should not be sent to Sentry.
/// </summary>
public class DisCatSharpExceptionFilter : IExceptionFilter
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DisCatSharpExceptionFilter" /> class.
	/// </summary>
	/// <param name="configuration">The discord configuration.</param>
	internal DisCatSharpExceptionFilter(DiscordConfiguration configuration)
	{
		this.Config = configuration;
	}

	/// <summary>
	///     Gets or sets the configuration.
	/// </summary>
	internal DiscordConfiguration Config { get; }

	/// <summary>
	///     Gets whether this filter shoulod track the exception.
	/// </summary>
	/// <param name="ex">The exception to check.</param>
	public bool Filter(Exception ex)
	{
		if (ex.Data[DiagnosticTags.ErrorOrigin] as string == DiagnosticTags.OriginLibrary)
			return false;

		var trackedException = ex is SentryCapturableException { InnerException: not null } wrapper
			? wrapper.InnerException
			: ex;

		return !this.Config.Telemetry.TrackExceptions.Contains(trackedException.GetType());
	}
}
