// Add the missing event args type
using System;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Provides data for global exception events.
/// </summary>
public class GlobalExceptionEventArgs(Exception exception, IServiceProvider provider) : DiscordEventArgs(provider)
{
	/// <summary>
	/// Gets the exception that occurred.
	/// </summary>
	public Exception Exception { get; } = exception;
}
