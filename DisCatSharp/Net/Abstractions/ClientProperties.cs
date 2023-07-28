// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
			return plat.Contains("freebsd") ? "freebsd" :
				plat.Contains("openbsd") ? "openbsd" :
				plat.Contains("netbsd") ? "netbsd" :
				plat.Contains("dragonfly") ? "dragonflybsd" :
				plat.Contains("miros bsd") || plat.Contains("mirbsd") ? "miros bsd" :
				plat.Contains("desktopbsd") ? "desktopbsd" :
				plat.Contains("darwin") ? "osx" :
				plat.Contains("unix") ? "unix" :
				"toaster (unknown)";
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
