using System;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Exposes the active runtime addresses for the optional self-hosted ASP.NET Core ingress mode.
/// </summary>
public sealed class DiscordAspNetCoreSelfHostRuntime
{
	/// <summary>
	///     Gets the bound listen base URL for the internal ASP.NET Core app when it is running.
	/// </summary>
	public Uri? ListenBaseUrl { get; private set; }

	/// <summary>
	///     Gets the externally visible base URL for the self-hosted ingress surface when it is running.
	/// </summary>
	public Uri? PublicBaseUrl { get; private set; }

	internal void SetRunning(Uri listenBaseUrl, Uri publicBaseUrl)
	{
		ArgumentNullException.ThrowIfNull(listenBaseUrl);
		ArgumentNullException.ThrowIfNull(publicBaseUrl);

		this.ListenBaseUrl = listenBaseUrl;
		this.PublicBaseUrl = publicBaseUrl;
	}

	internal void Reset()
	{
		this.ListenBaseUrl = null;
		this.PublicBaseUrl = null;
	}
}
