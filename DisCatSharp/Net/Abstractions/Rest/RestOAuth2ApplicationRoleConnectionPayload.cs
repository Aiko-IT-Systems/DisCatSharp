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

using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents the oauth2 application role connection payload.
/// </summary>
internal sealed class RestOAuth2ApplicationRoleConnectionPayload : ObservableApiObject
{
	/// <summary>
	/// Sets the role connections new platform name.
	/// </summary>
	[JsonProperty("platform_name")]
	public Optional<string> PlatformName { internal get; set; }

	/// <summary>
	/// Sets the role connections new platform username.
	/// </summary>
	[JsonProperty("platform_username")]
	public Optional<string> PlatformUsername { internal get; set; }

	/// <summary>
	/// Sets the role connections new metadata.
	/// </summary>
	[JsonProperty("metadata")]
	public Optional<Dictionary<string, string>> Metadata { internal get; set; }
}
