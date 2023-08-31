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

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities.OAuth2;

/// <summary>
/// Represents a <see cref="DiscordAuthorizationInformation"/>.
/// </summary>
public sealed class DiscordAuthorizationInformation : ObservableApiObject
{
	/// <summary>
	/// Gets the current application.
	/// </summary>
	[JsonProperty("application")]
	public DiscordApplication Application { get; internal set; }

	/// <summary>
	/// Gets the scopes the user has authorized the application for.
	/// </summary>
	[JsonProperty("scopes")]
	public List<string> Scopes { get; internal set; } = new();

	/// <summary>
	/// Gets when the access token expires as raw string.
	/// </summary>
	[JsonProperty("expires")]
	internal string ExpiresRaw { get; set; }

	/// <summary>
	/// Gets when the access token expires.
	/// </summary>
	[JsonIgnore]
	internal DateTimeOffset Expires
		=> DateTimeOffset.TryParse(this.ExpiresRaw, out var expires)
			? expires
			: throw new InvalidCastException("Something went wrong");

	/// <summary>
	/// Gets the user who has authorized, if the user has authorized with the <c>identify</c> scope.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser? User { get; internal set; }
}
