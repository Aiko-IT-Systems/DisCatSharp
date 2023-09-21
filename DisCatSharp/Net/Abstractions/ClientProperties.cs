using System.Reflection;
using System.Runtime.InteropServices;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents data for identify payload's client properties.
/// </summary>
internal sealed class ClientProperties : ObservableApiObject
{
	/// <summary>
	/// Gets the client's operating system.
	/// </summary>
	[JsonProperty("os")]
	public string OperatingSystem
	{
		get
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return "windows";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return "linux";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return "osx";

			var plat = RuntimeInformation.OSDescription.ToLowerInvariant();
			return plat.Contains("freebsd")
				       ? "freebsd"
				       : plat.Contains("openbsd")
					       ? "openbsd"
					       : plat.Contains("netbsd")
						       ? "netbsd"
						       : plat.Contains("dragonfly")
							       ? "dragonflybsd"
							       : plat.Contains("miros bsd") || plat.Contains("mirbsd")
								       ? "miros bsd"
								       : plat.Contains("desktopbsd")
									       ? "desktopbsd"
									       : plat.Contains("darwin")
										       ? "osx"
										       : plat.Contains("unix")
											       ? "unix"
											       : "toaster (unknown)";
		}
	}

	/// <summary>
	/// Gets the client's browser.
	/// </summary>
	[JsonProperty("browser")]
	public string Browser
	{
		get
		{
			if (this.Discord.Configuration.MobileStatus)
				return "Discord Android";

			var a = typeof(DiscordClient).GetTypeInfo().Assembly;
			var an = a.GetName();
			return $"DisCatSharp {an.Version.ToString(4)}";
		}
	}

	/// <summary>
	/// Gets the client's device.
	/// </summary>
	[JsonProperty("device")]
	public string Device
		=> this.Browser;

	/// <summary>
	/// Gets the client's referrer.
	/// </summary>
	[JsonProperty("referrer")]
	public string Referrer
		=> "";

	/// <summary>
	/// Gets the client's referring domain.
	/// </summary>
	[JsonProperty("referring_domain")]
	public string ReferringDomain
		=> "";
}
