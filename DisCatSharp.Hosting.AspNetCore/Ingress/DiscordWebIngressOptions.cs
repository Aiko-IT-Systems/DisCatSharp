using System;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Configures transport-agnostic Discord web ingress services.
/// </summary>
public sealed class DiscordWebIngressOptions
{
	/// <summary>
	///     The default maximum request body size in bytes.
	/// </summary>
	public const int DefaultMaxRequestBodySize = 256 * 1024;

	/// <summary>
	///     Gets or sets the hex-encoded Discord application verify key used for signed ingress validation.
	/// </summary>
	public string? ApplicationVerifyKey { get; set; }

	/// <summary>
	///     Gets or sets the maximum allowed size of a raw ingress request body in bytes.
	/// </summary>
	public int MaxRequestBodySize { get; set; } = DefaultMaxRequestBodySize;

	/// <summary>
	///     Gets or sets the default lifetime applied to pending ingress state entries.
	/// </summary>
	public TimeSpan PendingStateLifetime { get; set; } = TimeSpan.FromMinutes(15);

	/// <summary>
	///     Gets or sets how often the default in-memory pending state store prunes expired entries.
	/// </summary>
	public TimeSpan PendingStateCleanupInterval { get; set; } = TimeSpan.FromMinutes(5);
}
