using System;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a failure caused by an ingress request body exceeding the configured maximum size.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DiscordIngressBodyTooLargeException" /> class.
/// </remarks>
/// <param name="maxAllowedBytes">The configured maximum request body size, in bytes.</param>
public sealed class DiscordIngressBodyTooLargeException(int maxAllowedBytes)
	: InvalidOperationException($"The ingress request body exceeded the configured maximum size of {maxAllowedBytes} bytes.")
{

	/// <summary>
	///     Gets the configured maximum request body size, in bytes.
	/// </summary>
	public int MaxAllowedBytes { get; } = maxAllowedBytes;
}
