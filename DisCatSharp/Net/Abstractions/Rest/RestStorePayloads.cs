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

/// <summary>
/// Represents a test entitlement create payload.
/// </summary>
internal sealed class TestEntitlementCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the sku to grant entitlement to, as Discord for this value.
	/// </summary>
	[JsonProperty("sku_id")]
	public ulong SkuId { get; set; }

	/// <summary>
	/// Gets or sets the guild id or user id to grant entitlement to.
	/// </summary>
	[JsonProperty("owner_id")]
	public ulong OwnerId { get; set; }

	/// <summary>
	/// Gets or sets the type of subscription (guild, user).
	/// </summary>
	[JsonProperty("owner_type")]
	public EntitlementOwnerType OwnerType { get; set; }
}
