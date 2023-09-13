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
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a <see cref="DiscordSignedLink"/> used for attachments and other things to improve security
/// and prevent bad actors from abusing Discord's CDN.
/// </summary>
public sealed class DiscordSignedLink : Uri
{
	/// <summary>
	/// When the signed link expires.
	/// </summary>
	public DateTimeOffset? ExpiresAt { get; init; }

	/// <summary>
	/// When the signed link was generated.
	/// </summary>
	public DateTimeOffset? IssuedAt { get; init; }

	/// <summary>
	/// The signature of the signed link.
	/// </summary>
	public string? Signature { get; init; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Uri"/> class with the specified URI for signed discord links.
	/// </summary>
	/// <param name="uriString">A string that identifies the resource to be represented by the <see cref="Uri"/> instance.</param>
	public DiscordSignedLink(string uriString)
		: base(uriString)
	{
		if (string.IsNullOrWhiteSpace(this.Query))
			return;

		var queries = HttpUtility.ParseQueryString(this.Query);
		if (!queries.HasKeys())
		{
			return;
		}

		if (queries.Get("ex") is { } expiresString && long.TryParse(expiresString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var expiresTimeStamp))
		{
			this.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresTimeStamp);
		}

		if (queries.Get("is") is { } issuedString && long.TryParse(issuedString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var issuedTimeStamp))
		{
			this.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedTimeStamp);
		}

		this.Signature = queries.Get("sg");
	}
}

