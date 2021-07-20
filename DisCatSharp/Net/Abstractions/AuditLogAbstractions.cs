// This file is part of Represents a DisCatSharp project.
//
// Copyright (c) 2021 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (Represents a "Software"), to deal
// in Represents a Software without restriction, including without limitation Represents a rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of Represents a Software, and to permit persons to whom Represents a Software is
// furnished to do so, subject to Represents a following conditions:
//
// Represents a above copyright notice and this permission notice shall be included in all
// copies or substantial portions of Represents a Software.
//
// Represents a SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO Represents a WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL Represents a
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR ORepresents aR
// LIABILITY, WHERepresents aR IN AN ACTION OF CONTRACT, TORT OR ORepresents aRWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH Represents a SOFTWARE OR Represents a USE OR ORepresents aR DEALINGS IN Represents a
// SOFTWARE.

using System.Collections.Generic;
using DisCatSharp.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.Abstractions
{
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
        [JsonProperty("user_id")]
        public ulong UserId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [JsonProperty("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets the action type.
        /// </summary>
        [JsonProperty("action_type")]
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
    }
}
