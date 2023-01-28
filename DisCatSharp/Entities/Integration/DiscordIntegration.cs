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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord integration. These appear on the profile as linked 3rd party accounts.
/// </summary>
public class DiscordIntegration : SnowflakeObject
{
	/// <summary>
	/// Gets the integration name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the integration type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; internal set; }

	/// <summary>
	/// Gets whether this integration is enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsEnabled { get; internal set; }

	/// <summary>
	/// Gets whether this integration is syncing.
	/// </summary>
	[JsonProperty("syncing", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsSyncing { get; internal set; }

	/// <summary>
	/// Gets ID of the role this integration uses for subscribers.
	/// </summary>
	[JsonProperty("role_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong RoleId { get; internal set; }

	/// <summary>
	/// Gets the expiration behaviour.
	/// </summary>
	[JsonProperty("expire_behavior", NullValueHandling = NullValueHandling.Ignore)]
	public IntegrationExpireBehavior ExpireBehavior { get; internal set; }

	/// <summary>
	/// Gets the grace period before expiring subscribers.
	/// </summary>
	[JsonProperty("expire_grace_period", NullValueHandling = NullValueHandling.Ignore)]
	public int ExpireGracePeriod { get; internal set; }

	/// <summary>
	/// Gets the user that owns this integration.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the 3rd party service account for this integration.
	/// </summary>
	[JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordIntegrationAccount Account { get; internal set; }

	/// <summary>
	/// Gets the date and time this integration was last synced.
	/// </summary>
	[JsonProperty("synced_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset SyncedAt { get; internal set; }

	/// <summary>
	/// Gets the subscriber count.
	/// </summary>
	[JsonProperty("subscriber_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? SubscriberCount { get; internal set; }

	/// <summary>
	/// Whether the integration is revoked.
	/// </summary>
	[JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Revoked { get; internal set; }

	[JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordApplication Application { get; internal set; }

	[JsonProperty("scopes", NullValueHandling = NullValueHandling.Ignore)]
	public string[] Scopes { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordIntegration"/> class.
	/// </summary>
	internal DiscordIntegration()
	{ }
}
