// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a audit log user.
/// </summary>
internal sealed class AuditLogUser
{
	/// <summary>
	/// Gets or sets the username.
	/// </summary>
	[JsonProperty("username")]
	public string Username { get; set; }

	/// <summary>
	/// Gets or sets the discriminator.
	/// </summary>
	[JsonProperty("discriminator")]
	public string Discriminator { get; set; }

	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets or sets the avatar hash.
	/// </summary>
	[JsonProperty("avatar")]
	public string AvatarHash { get; set; }
}

/// <summary>
/// Represents a audit log webhook.
/// </summary>
internal sealed class AuditLogWebhook
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public ulong ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the token.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; set; }

	/// <summary>
	/// Gets or sets the avatar hash.
	/// </summary>
	[JsonProperty("avatar")]
	public string AvatarHash { get; set; }

	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }

	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }
}

/// <summary>
/// Represents a audit log thread metadata.
/// </summary>
internal sealed class AuditLogThreadMetadata
{
	/// <summary>
	/// Gets whether the thread is archived.
	/// </summary>
	[JsonProperty("archived")]
	public bool Archived { get; set; }

	/// <summary>
	/// Gets the threads archive timestamp.
	/// </summary>
	[JsonProperty("archive_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public string ArchiveTimestamp { get; set; }

	/// <summary>
	/// Gets the threads auto archive duration.
	/// </summary>
	[JsonProperty("auto_archive_duration")]
	public int AutoArchiveDuration { get; set; }

	/// <summary>
	/// Gets whether the thread is locked.
	/// </summary>
	[JsonProperty("locked")]
	public bool Locked { get; set; }
}

/// <summary>
/// Represents a audit log thread.
/// </summary>
internal sealed class AuditLogThread
{
	/// <summary>
	/// Gets the thread id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets the thread guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }

	/// <summary>
	/// Gets the thread parent channel id.
	/// </summary>
	[JsonProperty("parent_id")]
	public ulong ParentId { get; set; }

	/// <summary>
	/// Gets the thread owner id.
	/// </summary>
	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OwnerId { get; set; }

	/// <summary>
	/// Gets the thread type.
	/// </summary>
	[JsonProperty("type")]
	public ChannelType Type { get; set; }

	/// <summary>
	/// Gets the thread name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the thread last message id.
	/// </summary>
	[JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? LastMessageId { get; set; }

	/// <summary>
	/// Gets the thread metadata.
	/// </summary>
	[JsonProperty("thread_metadata")]
	public AuditLogThreadMetadata Metadata { get; set; }

	/// <summary>
	/// Gets the thread approximate message count.
	/// </summary>
	[JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? MessageCount { get; set; }

	/// <summary>
	/// Gets the thread member count.
	/// </summary>
	[JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? MemberCount { get; set; }

	/// <summary>
	/// Gets the thread rate limit per user.
	/// </summary>
	[JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public int? RateLimitPerUser { get; set; }
}

/// <summary>
/// Represents a audit log scheduled event.
/// </summary>
internal sealed class AuditLogGuildScheduledEvent
{
	/// <summary>
	/// Gets the scheduled event id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets the scheduled event guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }

	/// <summary>
	/// Gets the scheduled event channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; set; }

	/// <summary>
	/// Gets the scheduled event creator id.
	/// </summary>
	[JsonProperty("creator_id")]
	public ulong CreatorId { get; set; }

	/// <summary>
	/// Gets the scheduled event name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the scheduled event description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; }

	/// <summary>
	/// Gets the scheduled event image.
	/// </summary>
	[JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
	public string Image { get; set; }

	/// <summary>
	/// Gets the scheduled event scheduled start time.
	/// </summary>
	[JsonProperty("scheduled_start_time")]
	public string ScheduledStartTime;

	/// <summary>
	/// Gets the scheduled event scheduled end time.
	/// </summary>
	[JsonProperty("scheduled_end_time")]
	public string ScheduledEndTime { get; set; }

	/// <summary>
	/// Gets the scheduled event privacy level.
	/// </summary>
	[JsonProperty("privacy_level")]
	public ScheduledEventPrivacyLevel PrivacyLevel { get; set; }

	/// <summary>
	/// Gets the scheduled event status.
	/// </summary>
	[JsonProperty("status")]
	public ScheduledEventStatus Status { get; set; }

	/// <summary>
	/// Gets the scheduled event entity type.
	/// </summary>
	[JsonProperty("entity_type")]
	public ScheduledEventEntityType EntityType { get; set; }

	/// <summary>
	/// Gets the scheduled event entity id.
	/// </summary>
	[JsonProperty("entity_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong EntityId { get; set; }

	/// <summary>
	/// Gets the scheduled event entity metadata.
	/// </summary>
	[JsonProperty("entity_metadata")]
	public AuditLogGuildScheduledEventEntityMetadata EntityMetadata { get; set; }

	/// <summary>
	/// Gets the scheduled event sku ids.
	/// </summary>
	[JsonProperty("sku_ids")]
	public List<ulong> SkuIds { get; set; }
}

/// <summary>
/// Represents a audit log scheduled event entity metadata.
/// </summary>
internal sealed class AuditLogGuildScheduledEventEntityMetadata
{
	/// <summary>
	/// Gets the scheduled events external location.
	/// </summary>
	[JsonProperty("location")]
	public string Location { get; set; }
}

/// <summary>
/// Represents a audit log integration account.
/// </summary>
internal sealed class AuditLogIntegrationAccount
{
	/// <summary>
	/// Gets the account id.
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; set; }

	/// <summary>
	/// Gets the account name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }
}

/// <summary>
/// Represents a audit log integration.
/// </summary>
internal sealed class AuditLogIntegration
{
	/// <summary>
	/// Gets the integration id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets the integration type.
	/// </summary>
	[JsonProperty("type")]
	public string Type { get; set; }

	/// <summary>
	/// Gets the integration name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the integration account.
	/// </summary>
	[JsonProperty("account")]
	public AuditLogIntegrationAccount Account { get; set; }
}

/// <summary>
/// Represents a audit log action change.
/// </summary>
internal sealed class AuditLogActionChange
{
	// this can be a string or an array
	/// <summary>
	/// Gets or sets the old value.
	/// </summary>
	[JsonProperty("old_value")]
	public object OldValue { get; set; }

	/// <summary>
	/// Gets the old values.
	/// </summary>
	[JsonIgnore]
	public IEnumerable<JObject> OldValues
		=> (this.OldValue as JArray)?.ToObject<IEnumerable<JObject>>();

	/// <summary>
	/// Gets the old value ulong.
	/// </summary>
	[JsonIgnore]
	public ulong OldValueUlong
		=> (ulong)this.OldValue;

	/// <summary>
	/// Gets the old value string.
	/// </summary>
	[JsonIgnore]
	public string OldValueString
		=> (string)this.OldValue;

	// this can be a string or an array
	/// <summary>
	/// Gets or sets the new value.
	/// </summary>
	[JsonProperty("new_value")]
	public object NewValue { get; set; }

	/// <summary>
	/// Gets the new values.
	/// </summary>
	[JsonIgnore]
	public IEnumerable<JObject> NewValues
		=> (this.NewValue as JArray)?.ToObject<IEnumerable<JObject>>();

	/// <summary>
	/// Gets the new value ulong.
	/// </summary>
	[JsonIgnore]
	public ulong NewValueUlong
		=> (ulong)this.NewValue;

	/// <summary>
	/// Gets the new value string.
	/// </summary>
	[JsonIgnore]
	public string NewValueString
		=> (string)this.NewValue;

	/// <summary>
	/// Gets or sets the key.
	/// </summary>
	[JsonProperty("key")]
	public string Key { get; set; }
}

/// <summary>
/// Represents a audit log action options.
/// </summary>
internal sealed class AuditLogActionOptions
{
	/// <summary>
	/// Gets or sets the type.
	/// </summary>
	[JsonProperty("type")]
	public object Type { get; set; }

	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public ulong ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the message id.
	/// </summary>
	[JsonProperty("message_id")]
	public ulong MessageId { get; set; }

	/// <summary>
	/// Gets or sets the count.
	/// </summary>
	[JsonProperty("count")]
	public int Count { get; set; }

	/// <summary>
	/// Gets or sets the delete member days.
	/// </summary>
	[JsonProperty("delete_member_days")]
	public int DeleteMemberDays { get; set; }

	/// <summary>
	/// Gets or sets the members removed.
	/// </summary>
	[JsonProperty("members_removed")]
	public int MembersRemoved { get; set; }
}

/// <summary>
/// Represents a audit log action.
/// </summary>
internal sealed class AuditLogAction
{
	/// <summary>
	/// Gets or sets the target id.
	/// </summary>
	[JsonProperty("target_id")]
	public ulong? TargetId { get; set; }

	/// <summary>
	/// Gets or sets the user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong UserId { get; set; }

	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets or sets the action type.
	/// </summary>
	[JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
	public AuditLogActionType ActionType { get; set; }

	/// <summary>
	/// Gets or sets the changes.
	/// </summary>
	[JsonProperty("changes")]
	public IEnumerable<AuditLogActionChange> Changes { get; set; }

	/// <summary>
	/// Gets or sets the options.
	/// </summary>
	[JsonProperty("options")]
	public AuditLogActionOptions Options { get; set; }

	/// <summary>
	/// Gets or sets the reason.
	/// </summary>
	[JsonProperty("reason")]
	public string Reason { get; set; }
}

/// <summary>
/// Represents a audit log.
/// </summary>
internal sealed class AuditLog
{
	/// <summary>
	/// Gets or sets the webhooks.
	/// </summary>
	[JsonProperty("webhooks")]
	public IEnumerable<AuditLogWebhook> Webhooks { get; set; }

	/// <summary>
	/// Gets or sets the users.
	/// </summary>
	[JsonProperty("users")]
	public IEnumerable<AuditLogUser> Users { get; set; }

	/// <summary>
	/// Gets or sets the entries.
	/// </summary>
	[JsonProperty("audit_log_entries")]
	public IEnumerable<AuditLogAction> Entries { get; set; }

	/// <summary>
	/// Gets or sets the scheduled events.
	/// </summary>
	[JsonProperty("guild_scheduled_events")]
	public IEnumerable<AuditLogGuildScheduledEvent> ScheduledEvents { get; set; }

	/// <summary>
	/// Gets or sets the threads.
	/// </summary>
	[JsonProperty("threads")]
	public IEnumerable<AuditLogThread> Threads { get; set; }

	/// <summary>
	/// Gets or sets the integrations.
	/// Twitch related.
	/// </summary>
	[JsonProperty("integrations")]
	public IEnumerable<AuditLogIntegration> Integrations { get; set; }

	/*
        /// <summary>
        /// Gets or sets the application commands.
        /// Related to Permissions V2.
        /// </summary>
        [JsonProperty("application_commands")]
        public IEnumerable<AuditLogApplicationCommand> ApplicationCommands { get; set; }
        */
}
