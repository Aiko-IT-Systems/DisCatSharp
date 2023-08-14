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

using Newtonsoft.Json;

namespace DisCatSharp.Entities.OAuth2;

/// <summary>
/// Represents a <see cref="DiscordAccessToken"/>.
/// </summary>
public sealed class DiscordAccessToken : ObservableApiObject
{
	/// <summary>
	/// Gets the access token.
	/// </summary>
	[JsonProperty("access_token")]
	public string AccessToken { get; internal set; }

	/// <summary>
	/// Gets the token type.
	/// </summary>
	[JsonProperty("token_type")]
	public string TokenType { get; internal set; }

	/// <summary>
	/// Gets when the token expires.
	/// </summary>
	[JsonProperty("expires_in")]
	public int ExpiresIn { get; internal set; }

	/// <summary>
	/// Gets the refresh token.
	/// </summary>
	[JsonProperty("refresh_token")]
	public string RefreshToken { get; internal set; }

	/// <summary>
	/// Gets the scope.
	/// </summary>
	[JsonProperty("scope")]
	public string Scope { get; internal set; }
}
