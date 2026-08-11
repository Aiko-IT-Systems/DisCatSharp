using System;
using System.Net;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
///     Configures a <see cref="DiscordOAuth2Client" />.
/// </summary>
public sealed class DiscordOAuth2ClientConfiguration
{
	/// <summary>
	///     Sets the Discord application client ID.
	/// </summary>
	public ulong ClientId { internal get; set; }

	/// <summary>
	///     Sets the Discord application client secret.
	/// </summary>
	public string ClientSecret { internal get; set; } = null!;

	/// <summary>
	///     Sets the OAuth2 redirect URI.
	/// </summary>
	public string RedirectUri { internal get; set; } = null!;

	/// <summary>
	///     Sets the service provider exposed by the client.
	///     Defaults to an empty service provider.
	/// </summary>
	public IServiceProvider? ServiceProvider { internal get; set; }

	/// <summary>
	///     Sets the proxy used for HTTP connections.
	///     Defaults to <see langword="null" />.
	/// </summary>
	public IWebProxy? Proxy { internal get; set; }

	/// <summary>
	///     Sets the timeout used for HTTP requests.
	///     Defaults to ten seconds. Use <see cref="System.Threading.Timeout.InfiniteTimeSpan" /> to disable timeouts.
	/// </summary>
	public TimeSpan? Timeout { internal get; set; }

	/// <summary>
	///     Sets whether the system clock is used to compute rate-limit resets.
	///     Defaults to <see langword="true" />.
	/// </summary>
	public bool UseRelativeRateLimit { internal get; set; } = true;

	/// <summary>
	///     Sets the logging factory used by the client.
	///     Defaults to the DisCatSharp logger factory.
	/// </summary>
	public ILoggerFactory? LoggerFactory { internal get; set; }

	/// <summary>
	///     Sets the minimum log level.
	///     Defaults to <see cref="LogLevel.Information" />.
	/// </summary>
	public LogLevel MinimumLogLevel { internal get; set; } = LogLevel.Information;

	/// <summary>
	///     Sets the timestamp format used by the default logger.
	/// </summary>
	public string LogTimestampFormat { internal get; set; } = "yyyy-MM-dd HH:mm:ss zzz";

	/// <summary>
	///     Sets the file that stores the shared OAuth2 RSA private key.
	///     Relative paths are resolved from the process working directory. Clients using the same file can read each
	///     other's secure states and retain that capability across restarts.
	/// </summary>
	/// <remarks>
	///     The file contains an unencrypted private key. Restrict file-system access to the application identity.
	/// </remarks>
	public string RsaKeyFilePath { internal get; set; } = "dcs_oauth_rsa.sdcs";
}
