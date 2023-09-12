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
using System.Web;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

public sealed class DiscordSignedLink : Uri
{
	/// <summary>
	/// Gets the client.
	/// </summary>
	internal BaseDiscordClient? Client { get; }

	/// <summary>
	/// Gets the timestamp indicating when the attachment URL will expire, after which point you'd need to retrieve another URL.
	/// </summary>
	internal string? ExpiresAtTimeStamp { get; }

	/// <summary>
	/// Gets the datetime when the <see cref="Signature"/> will expire.
	/// </summary>
	public DateTime? ExpiresAt
		=> !string.IsNullOrWhiteSpace(this.ExpiresAtTimeStamp) && int.TryParse(this.ExpiresAtTimeStamp, out var uts)
			? DateTime.UnixEpoch.AddSeconds(uts)
			: null;

	/// <summary>
	/// Gets the timestamp indicating when the URL was issued
	/// </summary>
	internal string? IssuedAtTimeStamp { get; }

	/// <summary>
	/// Gets the datetime when the <see cref="Signature"/> was generated at.
	/// </summary>
	public DateTime? IssuedAt
		=> !string.IsNullOrWhiteSpace(this.IssuedAtTimeStamp) && int.TryParse(this.IssuedAtTimeStamp, out var uts)
			? DateTime.UnixEpoch.AddSeconds(uts)
			: null;

	/// <summary>
	/// Gets the unique signature that remains valid until <see cref="ExpiresAt"/>.
	/// </summary>
	public string? Signature { get; internal set; }


	/// <summary>
	/// Initializes a new instance of the <see cref="Uri"/> class with the specified URI and inject the discord client.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="uriString">A string that identifies the resource to be represented by the <see cref="Uri"/> instance.</param>
	public DiscordSignedLink(BaseDiscordClient client, string uriString)
		: base(uriString)
	{
		this.Client = client;
		try
		{
			var query = HttpUtility.ParseQueryString(new Uri(uriString).Query);
			if (!query.HasKeys())
				return;

			this.ExpiresAtTimeStamp = query.Get("ex");
			this.IssuedAtTimeStamp = query.Get("is");
			this.Signature = query.Get("sg");
		}
		catch (Exception ex)
		{
			this.Client.Logger.LogWarning("Uh nuh, parsing the signed link failed: {exception}\n\nYou can use it as normal Uri tho!", ex);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Uri"/> class with the specified URI.
	/// </summary>
	/// <param name="uriString">A string that identifies the resource to be represented by the <see cref="Uri"/> instance.</param>
	public DiscordSignedLink(string uriString)
		: base(uriString)
	{
		try
		{
			var query = HttpUtility.ParseQueryString(new Uri(uriString).Query);
			if (!query.HasKeys())
				return;

			this.ExpiresAtTimeStamp = query.Get("ex");
			this.IssuedAtTimeStamp = query.Get("is");
			this.Signature = query.Get("sg");
		}
		catch (Exception ex)
		{
			Console.WriteLine("Uh nuh, parsing the signed link failed: " + ex + "\n\nYou can use it as normal Uri tho!");
		}
	}
}
