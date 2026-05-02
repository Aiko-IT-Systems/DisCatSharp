using System;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Configures the optional self-hosted ASP.NET Core ingress mode for DisCatSharp.
/// </summary>
public sealed class DiscordAspNetCoreSelfHostOptions
{
	/// <summary>
	///     The default listen address used by the internal ASP.NET Core app.
	/// </summary>
	public const string DefaultListenAddress = "127.0.0.1";

	/// <summary>
	///     The default listen port used by the internal ASP.NET Core app.
	/// </summary>
	public const int DefaultListenPort = 8080;

	/// <summary>
	///     The default URI scheme used to build the internal listen address.
	/// </summary>
	public const string DefaultScheme = "http";

	/// <summary>
	///     Gets or sets the address or host name that the internal ASP.NET Core app should bind to.
	/// </summary>
	public string ListenAddress { get; set; } = DefaultListenAddress;

	/// <summary>
	///     Gets or sets the port that the internal ASP.NET Core app should bind to.
	///     Specify <c>0</c> to request an ephemeral port from the operating system.
	/// </summary>
	public int ListenPort { get; set; } = DefaultListenPort;

	/// <summary>
	///     Gets or sets the URI scheme used to build the internal listen address.
	/// </summary>
	public string Scheme { get; set; } = DefaultScheme;

	/// <summary>
	///     Gets or sets the externally visible base URL advertised by the self-hosted ingress surface.
	///     When not set, the runtime falls back to the bound listen address.
	/// </summary>
	public Uri? BaseUrl { get; set; }
}
