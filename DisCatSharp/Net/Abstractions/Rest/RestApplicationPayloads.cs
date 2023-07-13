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
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

internal sealed class RestApplicationModifyPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> Description { get; set; }

	/// <summary>
	/// Gets or sets the interactions endpoint url.
	/// </summary>
	[JsonProperty("interactions_endpoint_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> InteractionsEndpointUrl { get; set; }

	/// <summary>
	/// Gets or sets the role connections verification url.
	/// </summary>
	[JsonProperty("role_connections_verification_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> RoleConnectionsVerificationUrl { get; set; }

	/// <summary>
	/// Gets or sets the custom install url.
	/// </summary>
	[JsonProperty("custom_install_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> CustomInstallUrl { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	[JsonProperty("tags", NullValueHandling = NullValueHandling.Include)]
	public Optional<List<string>?> Tags { get; set; }

	/// <summary>
	/// Gets or sets the icon base64.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> IconBase64 { get; set; }

	/// <summary>
	/// Gets or sets the cover image base64.
	/// </summary>
	[JsonProperty("cover_image", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> ConverImageBase64 { get; set; }

	/// <summary>
	/// Gets or sets the application flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ApplicationFlags> Flags { get; set; }

	/// <summary>
	/// Gets or sets the install params.
	/// </summary>
	[JsonProperty("install_params", NullValueHandling = NullValueHandling.Include)]
	public Optional<DiscordApplicationInstallParams?> InstallParams { get; set; }
}
