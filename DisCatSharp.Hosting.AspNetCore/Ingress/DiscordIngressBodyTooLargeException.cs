using System;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a failure caused by an ingress request body exceeding the configured maximum size.
/// </summary>
public sealed class DiscordIngressBodyTooLargeException : InvalidOperationException
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressBodyTooLargeException" /> class.
	/// </summary>
	/// <param name="maxAllowedBytes">The configured maximum request body size, in bytes.</param>
	public DiscordIngressBodyTooLargeException(int maxAllowedBytes)
		: base($"The ingress request body exceeded the configured maximum size of {maxAllowedBytes} bytes.")
		=> this.MaxAllowedBytes = maxAllowedBytes;

	/// <summary>
	///     Gets the configured maximum request body size, in bytes.
	/// </summary>
	public int MaxAllowedBytes { get; }
}
